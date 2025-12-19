using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public interface ISnapshotStore
    {
        Task SetSnapshot<TSnapshot>(TSnapshot snapshot, CancellationToken cancellationToken = default) where TSnapshot : class, ISnapshot;
        Task<TSnapshot> LoadSnapshot<TSnapshot>(Guid aggregateGlobalUId, CancellationToken cancellationToken = default) where TSnapshot : class, ISnapshot;
    }
}
