using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using System;

namespace CrystalSharp.MongoDb.Stores.Models
{
    public class SnapshotStoreData : DbSnapshotEntity
    {
        // This property is required here for the MongoDB "_id" column.
        public Guid Id { get; set; }
    }
}
