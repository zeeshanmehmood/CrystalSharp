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
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MsSql.Database;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.MsSql.Stores;
using CrystalSharp.Sagas;

namespace CrystalSharp.MsSql.Extensions
{
    public static class CrystalSharpAdapterMsSqlExtensions
    {
        public static ICrystalSharpAdapter AddMsSql<TDbContext>(this ICrystalSharpAdapter crystalSharpAdapter, MsSqlSettings settings)
            where TDbContext : DbContext
        {
            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>((options) =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                options = options.UseSqlServer(settings.ConnectionString);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IMsSqlEntityFrameworkCoreContext, MsSqlEntityFrameworkCoreContext>();

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMsSqlEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MsSqlSettings settings)
        {
            AddMsSqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = !string.IsNullOrEmpty(settings.Schema) ? true : false;
            string schema = string.IsNullOrEmpty(settings.Schema) ? nameof(DbSchema.Dbo).ToLower() : settings.Schema;

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

        public static ICrystalSharpAdapter AddMsSqlReadModelStore<TDbContext, TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MsSqlSettings settings)
            where TDbContext : DbContext
        {
            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>((options) =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                options = options.UseSqlServer(settings.ConnectionString);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<TKey>, MsSqlReadModelStore<TDbContext, TKey>>();

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMsSqlSagaStore(this ICrystalSharpAdapter crystalSharpAdapter,
            MsSqlSettings settings,
            params Type[] types)
        {
            AddMsSqlDatabaseMigrator(crystalSharpAdapter);

            bool useSchema = !string.IsNullOrEmpty(settings.Schema) ? true : false;
            string schema = string.IsNullOrEmpty(settings.Schema) ? nameof(DbSchema.Dbo).ToLower() : settings.Schema;
            Assembly[] assemblies = types.Select(t => t.Assembly).ToArray();

            crystalSharpAdapter.RegisterSagas(assemblies);

            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaStore>(s => new MsSqlSagaStore(settings.ConnectionString, useSchema, schema));

            return crystalSharpAdapter;
        }

        private static ICrystalSharpAdapter AddMsSqlDatabaseMigrator(ICrystalSharpAdapter crystalSharpAdapter)
        {
            crystalSharpAdapter.ServiceCollection.TryAddTransient<IMsSqlDatabaseMigrator, MsSqlDatabaseMigrator>();

            return crystalSharpAdapter;
        }
    }
}
