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

            AddKurrentDbEventStore<TKey>(crystalSharpAdapter, clientSettings);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddKurrentDbEventStore<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, KurrentDBClientSettings clientSettings)
        {
            RegisterEventStore<TKey>(crystalSharpAdapter, clientSettings);

            return crystalSharpAdapter;
        }

        private static ICrystalSharpAdapter RegisterEventStore<TKey>(ICrystalSharpAdapter crystalSharpAdapter, KurrentDBClientSettings clientSettings)
        {
            crystalSharpAdapter.Register<KurrentDBClient>(s => { return new KurrentDBClient(clientSettings); }, ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<IEventStorePersistence, KurrentDbPersistence>(ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<IAggregateEventStore<TKey>, KurrentDbAggregateEventStore<TKey>>(ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<ISnapshotStore, KurrentDbSnapshotStore>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }
    }
}
