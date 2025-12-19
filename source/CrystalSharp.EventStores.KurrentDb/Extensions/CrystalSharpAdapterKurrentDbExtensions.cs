using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.EventStores.KurrentDb.Stores;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;

namespace CrystalSharp.EventStores.KurrentDb.Extensions
{
    public static class CrystalSharpAdapterKurrentDbExtensions
    {
        public static ICrystalSharpAdapter AddKurrentDbEventStore<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, string eventStoreConnectionString)
        {
            KurrentDBClientSettings clientSettings = KurrentDBClientSettings.Create(eventStoreConnectionString);
            KurrentDBClient client = new(clientSettings);

            RegisterEventStore<TKey>(crystalSharpAdapter, client);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddKurrentDbEventStore<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, KurrentDBClientSettings clientSettings)
        {
            KurrentDBClient client = new(clientSettings);

            RegisterEventStore<TKey>(crystalSharpAdapter, client);

            return crystalSharpAdapter;
        }

        private static ICrystalSharpAdapter RegisterEventStore<TKey>(ICrystalSharpAdapter crystalSharpAdapter, KurrentDBClient client)
        {
            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s => new KurrentDbPersistence(client));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s =>
                new KurrentDbAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s =>
                new KurrentDbSnapshotStore(s.GetRequiredService<IEventStorePersistence>()));

            return crystalSharpAdapter;
        }
    }
}
