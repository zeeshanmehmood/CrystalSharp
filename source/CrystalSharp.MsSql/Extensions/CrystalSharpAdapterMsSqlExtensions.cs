using CrystalSharp.Common.Extensions;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors.Exceptions;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.MsSql.Settings;
using CrystalSharp.MsSql.Stores;
using CrystalSharp.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrystalSharp.MsSql.Extensions
{
    public static class CrystalSharpAdapterMsSqlExtensions
    {
        public static ICrystalSharpAdapter AddMsSql<TDbContext>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MsSqlSettings settings,
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

                options = options.UseSqlServer(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMsSqlEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MsSqlSettings settings)
        {
            RegisterMsSqlDatabaseMigratorIfRequired(crystalSharpAdapter);

            bool useSchema = settings.Schema.IsValidString();
            string schema = settings.Schema.IsValidString() ? settings.Schema : nameof(DbSchema.Dbo).ToLower();

            crystalSharpAdapter.Register<IEventStorePersistence>(s => { return new MsSqlEventStore(settings.ConnectionString, useSchema, schema);}, ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<ISnapshotStore>(s => { return new MsSqlSnapshotStore(settings.ConnectionString, useSchema, schema); }, ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<IAggregateEventStore<TKey>, MsSqlAggregateEventStore<TKey>>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMsSqlReadModelStore<TDbContext, TKey>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MsSqlSettings settings,
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
                    options = options.UseSqlServer(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
                }
                else
                {
                    options = options.UseSqlServer(settings.ConnectionString);
                }
            });
            crystalSharpAdapter.Register<IReadModelStore<TKey>, MsSqlReadModelStore<TDbContext, TKey>>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMsSqlSagaStore(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MsSqlSettings settings,
            params Type[] types)
        {
            RegisterMsSqlDatabaseMigratorIfRequired(crystalSharpAdapter);

            bool useSchema = settings.Schema.IsValidString();
            string schema = settings.Schema.IsValidString() ? settings.Schema : nameof(DbSchema.Dbo).ToLower();
            Assembly[] assemblies = [.. types.Select(t => t.Assembly)];

            crystalSharpAdapter.RegisterSagas(assemblies);
            crystalSharpAdapter.Register<ISagaStore>(s => { return new MsSqlSagaStore(settings.ConnectionString, useSchema, schema); }, ServiceLifetime.Scoped);

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

        private static ICrystalSharpAdapter RegisterMsSqlDatabaseMigratorIfRequired(ICrystalSharpAdapter crystalSharpAdapter)
        {
            ServiceDescriptor databaseMigratorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(MsSqlDatabaseMigrator));

            if (databaseMigratorDescriptor is null)
            {
                crystalSharpAdapter.TryRegister<IMsSqlDatabaseMigrator, MsSqlDatabaseMigrator>(ServiceLifetime.Transient);
            }

            return crystalSharpAdapter;
        }
    }
}
