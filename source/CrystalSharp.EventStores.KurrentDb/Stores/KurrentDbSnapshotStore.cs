using CrystalSharp.Common.Settings;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.EventStores.KurrentDb.Stores
{
    public class KurrentDbSnapshotStore(IEventStorePersistence eventStorePersistence) : ISnapshotStore
    {
        private readonly IEventStorePersistence _eventStorePersistence = eventStorePersistence;

        private const string SNAPSHOT_CLR_TYPE_HEADER = ReservedName.SnapshotClrTypeName;
        private const string COMMIT_ID_HEADER = ReservedName.CommitId;

        public async Task SetSnapshot<TSnapshot>(TSnapshot snapshot, CancellationToken cancellationToken = default)
            where TSnapshot : class, ISnapshot
        {
            IDictionary<string, object> headers = new Dictionary<string, object>
            {
                { COMMIT_ID_HEADER, snapshot.GlobalUId },
                { SNAPSHOT_CLR_TYPE_HEADER, snapshot.GetType().AssemblyQualifiedName }
            };
            string stream = _eventStorePersistence.IdToStreamName(snapshot.GetType(), snapshot.GlobalUId);
            long originalVersion = -1;

            try
            {
                TSnapshot existingSnapshot = await LoadSnapshot<TSnapshot>(snapshot.GlobalUId, cancellationToken).ConfigureAwait(false);
                long existingSnapshotVersion = existingSnapshot.SnapshotVersion + 1;
                snapshot.SnapshotVersion = existingSnapshotVersion;
                originalVersion = existingSnapshotVersion - 1;
            }
            catch (Exception exception)
            {
                if (exception is SnapshotNotFoundException || exception is SnapshotDeletedException)
                {
                    snapshot.SnapshotVersion = 0;
                    originalVersion = -1;
                }
            }

            long expectedVersion = _eventStorePersistence.GetExpectedVersion(originalVersion);
            IList<TSnapshot> list = [snapshot];
            IEnumerable<EventDataItem<TSnapshot>> eventsData = _eventStorePersistence.PrepareEventData(list, headers);

            await _eventStorePersistence.Store(stream, eventsData, expectedVersion, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TSnapshot> LoadSnapshot<TSnapshot>(Guid aggregateGlobalUId, CancellationToken cancellationToken = default)
            where TSnapshot : class, ISnapshot
        {
            string stream = _eventStorePersistence.IdToStreamName(typeof(TSnapshot), aggregateGlobalUId);
            TSnapshot snapshot = default;
            IEnumerable<TSnapshot> snapshots = null;

            try
            {
                snapshots = await _eventStorePersistence.Get<TSnapshot>(stream, cancellationToken).ConfigureAwait(false);

                if (snapshots is not null && snapshots.Any())
                {
                    snapshot = snapshots.OrderByDescending(x => x.SnapshotVersion).FirstOrDefault();
                }
            }
            catch (EventStoreStreamNotFoundException exception)
            {
                string errorMessage = "Snapshot stream not found. Check the Stream property of this exception.";

                throw new SnapshotNotFoundException(stream, ReservedErrorCode.SystemError, errorMessage, exception);
            }
            catch (EventStoreStreamDeletedException exception)
            {
                string errorMessage = "Cannot read from a deleted snapshot stream. Check the Stream property and inner exception of this exception.";

                throw new SnapshotDeletedException(stream, ReservedErrorCode.SystemError, errorMessage, exception);
            }

            return snapshot;
        }
    }
}
