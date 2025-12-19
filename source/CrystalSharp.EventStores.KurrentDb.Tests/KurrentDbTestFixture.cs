using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.EventStores.KurrentDb.Tests
{
    public class KurrentDbTestFixture : IntegrationTestBase, IDisposable
    {
        public IAggregateEventStore<int> EventStorePersistence { get; private set; }

        public KurrentDbTestFixture()
        {
            ConfigureKurrentDb();

            EventStorePersistence = GetService<IAggregateEventStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
