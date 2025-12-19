using CrystalSharp.Domain;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public interface ISnapshot
    {
        public Guid GlobalUId { get; set; }
        public long SnapshotVersion { get; set; }
        public long Version { get; set; }
        public EntityStatus EntityStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
