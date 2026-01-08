using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain.EventDispatching;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
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

                options = options.UseSqlServer(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMsSqlEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MsSqlSettings settings)
        {
            RegisterMsSqlDatabaseMigratorIfRequired(crystalSharpAdapter);

            bool useSchema = settings.Schema.IsValidString();
            string schema = settings.Schema.IsValidString() ? settings.Schema : nameof(DbSchema.Dbo).ToLower();

            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s =>
                new MsSqlEventStore(settings.ConnectionString,
                    useSchema,
                    schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s => new MsSqlSnapshotStore(settings.ConnectionString, useSchema, schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s =>
                new MsSqlAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));

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

                if (interceptorsToRegister.HasAny())
                {
                    options = options.UseSqlServer(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
                }
                else
                {
                    options = options.UseSqlServer(settings.ConnectionString);
                }
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<TKey>, MsSqlReadModelStore<TDbContext, TKey>>();

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
            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaStore>(s => new MsSqlSagaStore(settings.ConnectionString, useSchema, schema));

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
                crystalSharpAdapter.ServiceCollection.AddSingleton<DateTractionInterceptor>();
            }

            if (dispatchDomainEventsInterceptorDescriptor is null)
            {
                ValidateDbInterceptor(typeof(DispatchDomainEventsInterceptor));
                crystalSharpAdapter.ServiceCollection.AddSingleton<DispatchDomainEventsInterceptor>();
            }
        }

        private static ICrystalSharpAdapter RegisterMsSqlDatabaseMigratorIfRequired(ICrystalSharpAdapter crystalSharpAdapter)
        {
            ServiceDescriptor databaseMigratorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(MsSqlDatabaseMigrator));

            if (databaseMigratorDescriptor is null)
            {
                crystalSharpAdapter.ServiceCollection.TryAddTransient<IMsSqlDatabaseMigrator, MsSqlDatabaseMigrator>();
            }

            return crystalSharpAdapter;
        }
    }
}
