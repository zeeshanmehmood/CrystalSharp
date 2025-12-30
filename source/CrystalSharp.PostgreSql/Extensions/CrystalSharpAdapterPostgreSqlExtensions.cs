using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain.EventDispatching;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            crystalSharpAdapter.ServiceCollection.AddSingleton<DateTractionInterceptor>();
            crystalSharpAdapter.ServiceCollection.AddSingleton<DispatchDomainEventsInterceptor>();

            if (interceptors.HasAny())
            {
                foreach (Type interceptor in interceptors)
                {
                    ValidateDbInterceptor(interceptor);
                    crystalSharpAdapter.ServiceCollection.AddSingleton(interceptor);
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
            AddPostgreSqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = settings.Schema.IsValidString();
            string schema = settings.Schema.IsValidString() ? settings.Schema : nameof(DbSchema.Public).ToLower();

            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s => new PostgreSqlEventStore(settings.ConnectionString, useSchema, schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s => new PostgreSqlSnapshotStore(settings.ConnectionString, useSchema, schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s =>
                new PostgreSqlAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));

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
                    crystalSharpAdapter.ServiceCollection.AddSingleton(interceptor);
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

                options = options.UseNpgsql(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<TKey>, PostgreSqlReadModelStore<TDbContext, TKey>>();

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddPostgreSqlSagaStore(this ICrystalSharpAdapter crystalSharpAdapter,
            PostgreSqlSettings settings,
            params Type[] types)
        {
            AddPostgreSqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = settings.Schema.IsValidString();
            string schema = settings.Schema.IsValidString() ? settings.Schema : nameof(DbSchema.Public).ToLower();
            Assembly[] assemblies = [.. types.Select(t => t.Assembly)];

            crystalSharpAdapter.RegisterSagas(assemblies);
            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaStore>(s => new PostgreSqlSagaStore(settings.ConnectionString, useSchema, schema));

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

        private static ICrystalSharpAdapter AddPostgreSqlDatabaseMigrator(ICrystalSharpAdapter crystalSharpAdapter)
        {
            crystalSharpAdapter.ServiceCollection.TryAddTransient<IPostgreSqlDatabaseMigrator, PostgreSqlDatabaseMigrator>();

            return crystalSharpAdapter;
        }
    }
}
