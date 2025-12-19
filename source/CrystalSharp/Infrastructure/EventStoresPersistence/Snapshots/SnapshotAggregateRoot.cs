using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public abstract class SnapshotAggregateRoot<TKey, TSnapshot> : AggregateRoot<TKey>, ISnapshotAggregateRoot
        where TSnapshot : class, ISnapshot
    {
        public long SnapshotVersion { get; set; } = -1;

        public virtual async Task<bool> ShouldTakeSnapshot(CancellationToken cancellationToken = default)
        {
            bool takeSnapshot = false;

            if (typeof(TSnapshot).GetCustomAttribute(typeof(SnapshotAttribute)) is SnapshotAttribute snapshotAttribute)
            {
                if (snapshotAttribute.Frequency < 1)
                {
                    string stream = this.GetType().ToStreamName(this.GlobalUId);
                    string errorMessage = "The snapshot frequency must be greater than zero (\"0\") and cannot be negative. Check the Stream and Frequency properties of this exception.";

                    throw new SnapshotFrequencyException(stream, snapshotAttribute.Frequency, ReservedErrorCode.SystemError, errorMessage);
                }

                int frequency = snapshotAttribute.Frequency;
                long aggregateVersion = Version;
                takeSnapshot = ++aggregateVersion % frequency == 0;
            }

            return await Task.FromResult(takeSnapshot);
        }

        public virtual async Task CreateSnapshot(ISnapshotStore snapshotStore, CancellationToken cancellationToken = default)
        {
            bool takeSnapshot = await ShouldTakeSnapshot(cancellationToken).ConfigureAwait(false);

            if (takeSnapshot)
            {
                TSnapshot snapshot = this.CopyTo<TSnapshot>();

                if (snapshot is not null)
                {
                    await snapshotStore.SetSnapshot(snapshot, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task<object> LoadSnapshot(ISnapshotStore snapshotStore,
            Guid aggregateStreamId,
            CancellationToken cancellationToken = default)
        {
            TSnapshot snapshot = await LoadSnapshotFromStore(snapshotStore, aggregateStreamId, cancellationToken).ConfigureAwait(false);

            return snapshot;
        }

        public virtual async Task<object> CopySnapshotTo(ISnapshotStore snapshotStore,
            Type aggregateRootType,
            CancellationToken cancellationToken = default)
        {
            TSnapshot snapshot = await LoadSnapshotFromStore(snapshotStore, this.GlobalUId, cancellationToken).ConfigureAwait(false);
            object copy = null;

            if (snapshot is not null)
            {
                copy = snapshot.CopyTo(aggregateRootType);
            }

            return copy;
        }

        private async Task<TSnapshot> LoadSnapshotFromStore(ISnapshotStore snapshotStore,
            Guid aggregateStreamId,
            CancellationToken cancellationToken = default)
        {
            TSnapshot snapshot = default;

            try
            {
                snapshot = await snapshotStore.LoadSnapshot<TSnapshot>(aggregateStreamId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (exception is SnapshotNotFoundException || exception is SnapshotDeletedException)
                {
                    snapshot = default;
                }
            }

            return snapshot;
        }
    }
}
