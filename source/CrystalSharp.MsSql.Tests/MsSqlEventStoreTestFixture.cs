using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.MsSql.Tests
{
    public class MsSqlEventStoreTestFixture : IntegrationTestBase, IDisposable
    {
        public IAggregateEventStore<int> EventStorePersistence { get; private set; }

        public MsSqlEventStoreTestFixture()
        {
            ConfigureMsSqlEventStore();

            EventStorePersistence = GetService<IAggregateEventStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
