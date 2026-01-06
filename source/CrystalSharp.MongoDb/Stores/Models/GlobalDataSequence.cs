using CrystalSharp.Common.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace CrystalSharp.MongoDb.Stores.Models
{
    public class GlobalDataSequence
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; }
        public long Value { get; set; }

        public void Insert(IMongoDatabase database, string sequenceName)
        {
            this.Name = sequenceName;

            ++this.Value;

            IMongoCollection<GlobalDataSequence> collection = database.GetCollection<GlobalDataSequence>(ReservedTableName.GlobalDataSequence);

            collection.InsertOne(this);
        }

        public long GetNextSequenceValue(IMongoDatabase database, string sequenceName)
        {
            IMongoCollection<GlobalDataSequence> collection = database.GetCollection<GlobalDataSequence>(ReservedTableName.GlobalDataSequence);
            FilterDefinition<GlobalDataSequence> filter = Builders<GlobalDataSequence>.Filter.Eq(a => a.Name, sequenceName);
            UpdateDefinition<GlobalDataSequence> update = Builders<GlobalDataSequence>.Update.Inc(a => a.Value, 1);
            GlobalDataSequence sequence = collection.FindOneAndUpdate(filter, update);

            return sequence.Value;
        }
    }
}
