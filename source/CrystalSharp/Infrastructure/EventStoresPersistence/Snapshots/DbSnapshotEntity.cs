using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public class DbSnapshotEntity : IDbSnapshotEntity
    {
        public Guid SnapshotId { get; set; }
        public string SnapshotAssembly { get; set; }
        public long SnapshotVersion { get; set; }
        public string StreamName { get; set; }
        public int EntityStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Data { get; set; }
    }
}
