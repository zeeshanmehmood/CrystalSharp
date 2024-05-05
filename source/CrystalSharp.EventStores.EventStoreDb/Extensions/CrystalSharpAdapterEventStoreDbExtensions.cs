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

using Microsoft.Extensions.DependencyInjection;
using EventStore.Client;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.EventStores.EventStoreDb.Stores;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;

namespace CrystalSharp.EventStores.EventStoreDb.Extensions
{
    public static class CrystalSharpAdapterEventStoreDbExtensions
    {
        public static ICrystalSharpAdapter AddEventStoreDbEventStore<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, string eventStoreConnectionString)
        {
            EventStoreClientSettings eventStoreClientSettings = EventStoreClientSettings.Create(eventStoreConnectionString);
            EventStoreClient eventStoreClient = new(eventStoreClientSettings);

            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s => new EventStoreDbPersistence(eventStoreClient));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s => 
                new EventStoreDbAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s => 
                new EventStoreDbSnapshotStore(s.GetRequiredService<IEventStorePersistence>()));

            return crystalSharpAdapter;
        }
    }
}
