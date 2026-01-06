using CrystalSharp.Infrastructure;

namespace CrystalSharp.MongoDb.Settings
{
    public class MongoDbSettings(string connectionString, string database) : DbSettings(connectionString)
    {
        public string Database { get; } = database;
    }
}
