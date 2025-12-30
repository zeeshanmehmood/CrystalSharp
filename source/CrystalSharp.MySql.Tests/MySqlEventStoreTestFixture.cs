using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.MySql.Tests
{
    public class MySqlEventStoreTestFixture : IntegrationTestBase, IDisposable
    {
        public IAggregateEventStore<int> EventStorePersistence { get; private set; }

        public MySqlEventStoreTestFixture()
        {
            ConfigureMySqlEventStore();

            EventStorePersistence = GetService<IAggregateEventStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
