// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MySql.Database;
using CrystalSharp.MySql.Migrator;
using CrystalSharp.MySql.Stores;
using CrystalSharp.Sagas;

namespace CrystalSharp.MySql.Extensions
{
    public static class CrystalSharpAdapterMySqlExtensions
    {
        public static ICrystalSharpAdapter AddMySql<TDbContext>(this ICrystalSharpAdapter crystalSharpAdapter, MySqlSettings settings)
            where TDbContext : DbContext
        {
            string connectionString = settings.ConnectionString;
            ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);

            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>(options =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                options = options.UseMySql(connectionString, serverVersion);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IMySqlEntityFrameworkCoreContext, MySqlEntityFrameworkCoreContext>();

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MySqlSettings settings)
        {
            AddMySqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = false;
            string schema = string.Empty;

            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s => 
                new MySqlEventStore(settings.ConnectionString, useSchema, schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s => 
                new MySqlSnapshotStore(settings.ConnectionString, useSchema, schema));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s => 
                new MySqlAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlReadModelStore<TDbContext, TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MySqlSettings settings)
            where TDbContext : DbContext
        {
            string connectionString = settings.ConnectionString;
            ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);

            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>(options =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                options = options.UseMySql(connectionString, serverVersion);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<TKey>, MySqlReadModelStore<TDbContext, TKey>>();

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMySqlSagaStore(this ICrystalSharpAdapter crystalSharpAdapter,
            MySqlSettings settings,
            params Type[] types)
        {
            AddMySqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = false;
            string schema = string.Empty;
            Assembly[] assemblies = types.Select(t => t.Assembly).ToArray();

            crystalSharpAdapter.RegisterSagas(assemblies);

            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaStore>(s => new MySqlSagaStore(settings.ConnectionString, useSchema, schema));

            return crystalSharpAdapter;
        }

        private static ICrystalSharpAdapter AddMySqlDatabaseMigrator(ICrystalSharpAdapter crystalSharpAdapter)
        {
            crystalSharpAdapter.ServiceCollection.TryAddTransient<IMySqlDatabaseMigrator, MySqlDatabaseMigrator>();

            return crystalSharpAdapter;
        }
    }
}
