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
using Microsoft.Extensions.DependencyInjection;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MongoDb.Database;
using CrystalSharp.MongoDb.Stores;
using CrystalSharp.Sagas;

namespace CrystalSharp.MongoDb.Extensions
{
    public static class CrystalSharpAdapterMongoDbExtensions
    {
        public static ICrystalSharpAdapter AddMongoDb(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            crystalSharpAdapter.ServiceCollection.AddScoped<IMongoDbContext>(s => new MongoDbContext(settings.ConnectionString,
                settings.Database,
                s.GetRequiredService<IEventDispatcher>()));

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            MongoDbEventStoreSetup.Run(settings.ConnectionString, settings.Database);

            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s => 
                new MongoDbEventStore(settings.ConnectionString, settings.Database));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s => 
                new MongoDbSnapshotStore(settings.ConnectionString, settings.Database));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s => 
                new MongoDbAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbReadModelStore(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<string>>(s => 
                new MongoDbReadModelStore(settings.ConnectionString, settings.Database));

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbSagaStore(this ICrystalSharpAdapter crystalSharpAdapter,
                MongoDbSettings settings,
                params Type[] types)
        {
            Assembly[] assemblies = types.Select(t => t.Assembly).ToArray();

            crystalSharpAdapter.RegisterSagas(assemblies);

            crystalSharpAdapter.ServiceCollection.AddScoped<IMongoDbSagaContext>(s => new MongoDbSagaContext(settings.ConnectionString, settings.Database));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaStore, MongoDbSagaStore>();

            return crystalSharpAdapter;
        }
    }
}
