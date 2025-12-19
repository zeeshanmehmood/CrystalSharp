using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate.Snapshots
{
    [Snapshot(nameof(ProductSnapshot), 3)]
    public class ProductSnapshot : SnapshotEntity<int>
    {
        public string Name { get; set; }
        public ProductInfo ProductInfo { get; set; }
    }
}
