using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.PostgreSql.Tests
{
    public class PostgreSqlEventStoreTestFixture : IntegrationTestBase, IDisposable
    {
        public IAggregateEventStore<int> EventStorePersistence { get; private set; }

        public PostgreSqlEventStoreTestFixture()
        {
            ConfigurePostgreSqlEventStore();

            EventStorePersistence = GetService<IAggregateEventStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
