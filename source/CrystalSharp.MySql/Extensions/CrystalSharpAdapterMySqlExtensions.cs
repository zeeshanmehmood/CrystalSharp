using CrystalSharp.Common.Extensions;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors.Exceptions;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MySql.Migrator;
using CrystalSharp.MySql.Settings;
using CrystalSharp.MySql.Stores;
using CrystalSharp.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrystalSharp.MySql.Extensions
{
    public static class CrystalSharpAdapterMySqlExtensions
    {
        public static ICrystalSharpAdapter AddMySql<TDbContext>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MySqlSettings settings,
            params Type[] interceptors)
            where TDbContext : DbContext
        {
            RegisterDefaultInterceptorsIfRequired(crystalSharpAdapter);

            if (interceptors.HasAny())
            {
                foreach (Type interceptor in interceptors)
                {
                    ValidateDbInterceptor(interceptor);
                    crystalSharpAdapter.Register(interceptor, ServiceLifetime.Singleton);
                }
            }

            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>((sp, options) =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                List<IInterceptor> interceptorsToRegister = [sp.GetRequiredService<DateTractionInterceptor>(), sp.GetRequiredService<DispatchDomainEventsInterceptor>()];

                if (interceptors.HasAny())
                {
                    foreach (Type interceptor in interceptors)
                    {
                        interceptorsToRegister.Add(sp.GetRequiredService(interceptor) as IInterceptor);
                    }
                }

                options = options.UseMySQL(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MySqlSettings settings)
        {
            RegisterMySqlDatabaseMigratorIfRequired(crystalSharpAdapter);

            bool useSchema = false;
            string schema = string.Empty;

            crystalSharpAdapter.Register<IEventStorePersistence>(s => { return new MySqlEventStore(settings.ConnectionString, useSchema, schema); }, ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<ISnapshotStore>(s => { return new MySqlSnapshotStore(settings.ConnectionString, useSchema, schema); }, ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<IAggregateEventStore<TKey>, MySqlAggregateEventStore<TKey>>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlReadModelStore<TDbContext, TKey>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MySqlSettings settings,
            params Type[] interceptors)
            where TDbContext : DbContext
        {
            if (interceptors.HasAny())
            {
                foreach (Type interceptor in interceptors)
                {
                    ValidateDbInterceptor(interceptor);
                    crystalSharpAdapter.Register(interceptor, ServiceLifetime.Singleton);
                }
            }

            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>((sp, options) =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                List<IInterceptor> interceptorsToRegister = [];

                if (interceptors.HasAny())
                {
                    foreach (Type interceptor in interceptors)
                    {
                        interceptorsToRegister.Add(sp.GetRequiredService(interceptor) as IInterceptor);
                    }
                }

                if (interceptorsToRegister.HasAny())
                {
                    options = options.UseMySQL(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
                }
                else
                {
                    options = options.UseMySQL(settings.ConnectionString);
                }
            });
            crystalSharpAdapter.Register<IReadModelStore<TKey>, MySqlReadModelStore<TDbContext, TKey>>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlSagaStore(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MySqlSettings settings,
            params Type[] types)
        {
            RegisterMySqlDatabaseMigratorIfRequired(crystalSharpAdapter);

            bool useSchema = false;
            string schema = string.Empty;
            Assembly[] assemblies = [.. types.Select(t => t.Assembly)];

            crystalSharpAdapter.RegisterSagas(assemblies);
            crystalSharpAdapter.Register<ISagaStore>(s => { return new MySqlSagaStore(settings.ConnectionString, useSchema, schema); }, ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        private static void ValidateDbInterceptor(Type interceptor)
        {
            if (!typeof(IInterceptor).IsAssignableFrom(interceptor))
            {
                string errorMessage = $"{interceptor.Name} must implement the {nameof(IInterceptor).ToDoubleQuotes()} interface of Entity Framework Core.";

                throw new InvalidDbInterceptorException(errorMessage);
            }
        }

        private static void RegisterDefaultInterceptorsIfRequired(ICrystalSharpAdapter crystalSharpAdapter)
        {
            ServiceDescriptor dateTractionInterceptorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(DateTractionInterceptor));
            ServiceDescriptor dispatchDomainEventsInterceptorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(DispatchDomainEventsInterceptor));

            if (dateTractionInterceptorDescriptor is null)
            {
                ValidateDbInterceptor(typeof(DateTractionInterceptor));
                crystalSharpAdapter.Register<DateTractionInterceptor>(ServiceLifetime.Singleton);
            }

            if (dispatchDomainEventsInterceptorDescriptor is null)
            {
                ValidateDbInterceptor(typeof(DispatchDomainEventsInterceptor));
                crystalSharpAdapter.Register<DispatchDomainEventsInterceptor>(ServiceLifetime.Singleton);
            }
        }

        private static ICrystalSharpAdapter RegisterMySqlDatabaseMigratorIfRequired(ICrystalSharpAdapter crystalSharpAdapter)
        {
            ServiceDescriptor databaseMigratorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(MySqlDatabaseMigrator));

            if (databaseMigratorDescriptor is null)
            {
                crystalSharpAdapter.TryRegister<IMySqlDatabaseMigrator, MySqlDatabaseMigrator>(ServiceLifetime.Transient);
            }

            return crystalSharpAdapter;
        }
    }
}
