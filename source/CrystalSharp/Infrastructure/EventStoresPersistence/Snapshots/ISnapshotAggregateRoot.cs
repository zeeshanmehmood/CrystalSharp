using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public interface ISnapshotAggregateRoot
    {
        Task<bool> ShouldTakeSnapshot(CancellationToken cancellationToken = default);
        Task CreateSnapshot(ISnapshotStore snapshotStore, CancellationToken cancellationToken = default);
        Task<object> LoadSnapshot(ISnapshotStore snapshotStore, Guid aggregateStreamId, CancellationToken cancellationToken = default);
        Task<object> CopySnapshotTo(ISnapshotStore snapshotStore, Type aggregateRootType, CancellationToken cancellationToken = default);
    }
}
