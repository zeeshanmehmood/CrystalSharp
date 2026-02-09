using CrystalSharp.Common.Extensions;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors.Exceptions;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.PostgreSql.Migrator;
using CrystalSharp.PostgreSql.Settings;
using CrystalSharp.PostgreSql.Stores;
using CrystalSharp.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrystalSharp.PostgreSql.Extensions
{
    public static class CrystalSharpAdapterPostgreSqlExtensions
    {
        public static ICrystalSharpAdapter AddPostgreSql<TDbContext>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            PostgreSqlSettings settings,
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

                options = options.UseNpgsql(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddPostgreSqlEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, PostgreSqlSettings settings)
        {
            RegisterPostgreSqlDatabaseMigratorIfRequired(crystalSharpAdapter);

            bool useSchema = settings.Schema.IsValidString();
            string schema = settings.Schema.IsValidString() ? settings.Schema : nameof(DbSchema.Public).ToLower();

            crystalSharpAdapter.Register<IEventStorePersistence>(s =>
            {
                return new PostgreSqlEventStore(settings.ConnectionString, useSchema, schema);
            },
            ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<ISnapshotStore>(s => { return new PostgreSqlSnapshotStore(settings.ConnectionString, useSchema, schema); }, ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<IAggregateEventStore<TKey>, PostgreSqlAggregateEventStore<TKey>>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddPostgreSqlReadModelStore<TDbContext, TKey>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            PostgreSqlSettings settings,
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
                    options = options.UseNpgsql(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
                }
                else
                {
                    options = options.UseNpgsql(settings.ConnectionString);
                }
            });
            crystalSharpAdapter.Register<IReadModelStore<TKey>, PostgreSqlReadModelStore<TDbContext, TKey>>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddPostgreSqlSagaStore(
            this ICrystalSharpAdapter crystalSharpAdapter,
            PostgreSqlSettings settings,
            params Type[] types)
        {
            RegisterPostgreSqlDatabaseMigratorIfRequired(crystalSharpAdapter);

            bool useSchema = settings.Schema.IsValidString();
            string schema = settings.Schema.IsValidString() ? settings.Schema : nameof(DbSchema.Public).ToLower();
            Assembly[] assemblies = [.. types.Select(t => t.Assembly)];

            crystalSharpAdapter.RegisterSagas(assemblies);
            crystalSharpAdapter.Register<ISagaStore>(s => { return new PostgreSqlSagaStore(settings.ConnectionString, useSchema, schema); }, ServiceLifetime.Scoped);

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

        private static ICrystalSharpAdapter RegisterPostgreSqlDatabaseMigratorIfRequired(ICrystalSharpAdapter crystalSharpAdapter)
        {
            ServiceDescriptor databaseMigratorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(PostgreSqlDatabaseMigrator));

            if (databaseMigratorDescriptor is null)
            {
                crystalSharpAdapter.TryRegister<IPostgreSqlDatabaseMigrator, PostgreSqlDatabaseMigrator>(ServiceLifetime.Transient);
            }

            return crystalSharpAdapter;
        }
    }
}
