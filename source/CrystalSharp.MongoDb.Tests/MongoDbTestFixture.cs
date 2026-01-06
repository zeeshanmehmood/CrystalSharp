using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MongoDb.Database;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.MongoDb.Tests
{
    public class MongoDbTestFixture : IntegrationTestBase, IDisposable
    {
        public IMongoDbContext MongoDbContext { get; private set; }
        public IAggregateEventStore<string> EventStorePersistence { get; private set; }
        public IReadModelStore<string> ReadModelStore { get; private set; }

        public MongoDbTestFixture()
        {
            ConfigureMongoDb("crystalsharp-testing", "crystalsharp-eventstore-testing", "crystalsharp-readmodel-store-testing");

            MongoDbContext = GetService<IMongoDbContext>();
            EventStorePersistence = GetService<IAggregateEventStore<string>>();
            ReadModelStore = GetService<IReadModelStore<string>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
