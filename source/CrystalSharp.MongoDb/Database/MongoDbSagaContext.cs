using MongoDB.Driver;

namespace CrystalSharp.MongoDb.Database
{
    public class MongoDbSagaContext : IMongoDbSagaContext
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbSagaContext(string connectionString, string database)
        {
            _mongoClient = new MongoClient(connectionString);
            _mongoDatabase = _mongoClient.GetDatabase(database);
        }

        public IMongoDatabase GetDbContext()
        {
            return _mongoDatabase;
        }
    }
}
