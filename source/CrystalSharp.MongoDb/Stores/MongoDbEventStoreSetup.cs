using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.MongoDb.Stores.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace CrystalSharp.MongoDb.Stores
{
    public static class MongoDbEventStoreSetup
    {
        public static void Run(string connectionString, string database)
        {
            MongoClient mongoClient = new(connectionString);
            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(database);
            IMongoCollection<GlobalDataSequence> dbCollection = mongoDatabase.GetCollection<GlobalDataSequence>(ReservedTableName.GlobalDataSequence);
            FilterDefinition<GlobalDataSequence> filter = Builders<GlobalDataSequence>.Filter.Eq(x => x.Name, ReservedColumnName.GlobalSequence);
            IEnumerable<GlobalDataSequence> rows = dbCollection.Find(filter).ToEnumerable();

            if (!rows.HasAny())
            {
                GlobalDataSequence globalDataSequence = new();

                globalDataSequence.Insert(mongoDatabase, ReservedColumnName.GlobalSequence);
            }
        }
    }
}
