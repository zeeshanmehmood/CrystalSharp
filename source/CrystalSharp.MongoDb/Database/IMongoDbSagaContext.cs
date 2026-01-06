using MongoDB.Driver;

namespace CrystalSharp.MongoDb.Database
{
    public interface IMongoDbSagaContext
    {
        IMongoDatabase GetDbContext();
    }
}
