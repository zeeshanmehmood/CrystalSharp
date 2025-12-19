using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Snapshots
{
    [Snapshot(nameof(CourseSnapshot), 3)]
    public class CourseSnapshot : SnapshotEntity<string>
    {
        public string Name { get; set; }
        public CourseInfo CourseInfo { get; set; }
    }
}
