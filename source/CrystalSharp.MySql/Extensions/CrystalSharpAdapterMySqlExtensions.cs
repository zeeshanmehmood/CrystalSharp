using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain.EventDispatching;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
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

                options = options.UseMySQL(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MySqlSettings settings)
        {
            AddMySqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = false;
            string schema = string.Empty;

            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s => new MySqlEventStore(settings.ConnectionString, useSchema, schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s => new MySqlSnapshotStore(settings.ConnectionString, useSchema, schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s =>
                new MySqlAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));

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

                options = options.UseMySQL(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<TKey>, MySqlReadModelStore<TDbContext, TKey>>();

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlSagaStore(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MySqlSettings settings,
            params Type[] types)
        {
            AddMySqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = false;
            string schema = string.Empty;
            Assembly[] assemblies = [.. types.Select(t => t.Assembly)];

            crystalSharpAdapter.RegisterSagas(assemblies);
            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaStore>(s => new MySqlSagaStore(settings.ConnectionString, useSchema, schema));

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

        private static ICrystalSharpAdapter AddMySqlDatabaseMigrator(ICrystalSharpAdapter crystalSharpAdapter)
        {
            crystalSharpAdapter.ServiceCollection.TryAddTransient<IMySqlDatabaseMigrator, MySqlDatabaseMigrator>();

            return crystalSharpAdapter;
        }
    }
}
