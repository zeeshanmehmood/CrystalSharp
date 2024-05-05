// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public abstract class SqlSnapshotStorePersistence : EventStorePersistence
    {
        private readonly IDbManager _dbManager;
        private readonly EventStoreQuery _eventStoreQuery;

        protected SqlSnapshotStorePersistence(IDbManager dbManager, EventStoreQuery eventStoreQuery)
        {
            _dbManager = dbManager;
            _eventStoreQuery = eventStoreQuery;
        }

        protected abstract IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters);

        public async Task SetSnapshot<TSnapshot>(TSnapshot snapshot, CancellationToken cancellationToken = default)
            where TSnapshot : class, ISnapshot
        {
            string stream = IdToStreamName(snapshot.GetType(), snapshot.GlobalUId);
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

            long expectedVersion = GetExpectedVersion(originalVersion);
            long lastVersion = await GetLastVersion(stream).ConfigureAwait(false);

            if (lastVersion >= expectedVersion)
            {
                string errorMessage = $"The expected version {expectedVersion} of the snapshot already exists. Check the Stream, LastVersion and ExpectedVersion properties of this exception.";

                throw new SnapshotVersionConflictException(stream, lastVersion, expectedVersion, ReservedErrorCode.SystemError, errorMessage);
            }

            DbSnapshotEntity dbSnapshotEntity = new()
            {
                SnapshotId = Guid.NewGuid(),
                EntityStatus = 1,
                CreatedOn = SystemDate.UtcNow,
                SnapshotVersion = snapshot.SnapshotVersion,
                StreamName = stream,
                SnapshotAssembly = snapshot.GetType().AssemblyQualifiedName,
                Data = Serializer.Serialize(snapshot)
            };

            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.SetSnapshotQuery(dbSnapshotEntity.SnapshotId,
                dbSnapshotEntity.SnapshotAssembly,
                dbSnapshotEntity.SnapshotVersion,
                dbSnapshotEntity.StreamName,
                dbSnapshotEntity.EntityStatus,
                dbSnapshotEntity.CreatedOn,
                dbSnapshotEntity.Data);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);

            await _dbManager.ExecuteNonQuery(query, parameters).ConfigureAwait(false);
        }

        public async Task<TSnapshot> LoadSnapshot<TSnapshot>(Guid aggregateGlobalUId, CancellationToken cancellationToken = default)
            where TSnapshot : class, ISnapshot
        {
            string stream = IdToStreamName(typeof(TSnapshot), aggregateGlobalUId);
            TSnapshot snapshot = default;
            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.LoadSnapshotQuery(stream);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);
            IEnumerable<IDbSnapshotEntity> snapshots = await _dbManager.ExecuteQuery<DbSnapshotEntity>(query, parameters).ConfigureAwait(false);

            if (!snapshots.HasAny())
            {
                string errorMessage = "Snapshot stream not found. Check the Stream property of this exception.";

                throw new SnapshotNotFoundException(stream, ReservedErrorCode.SystemError, errorMessage);
            }

            IDbSnapshotEntity dbSnapshotEntity = snapshots.FirstOrDefault();

            if (dbSnapshotEntity != null)
            {
                snapshot = DeserializeSnapshot<TSnapshot>(dbSnapshotEntity.Data, dbSnapshotEntity.SnapshotAssembly);
            }

            return snapshot;
        }

        public override long GetExpectedVersion(long originalVersion)
        {
            return ++originalVersion;
        }

        private async Task<long> GetLastVersion(string stream)
        {
            long lastVersion = -1;
            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.GetSnapshotLastVersionQuery(stream);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);
            object result = await _dbManager.ExecuteScalar(query, parameters).ConfigureAwait(false);

            if (result != null)
            {
                lastVersion = (long)result;
            }

            return lastVersion;
        }

        private TSnapshot DeserializeSnapshot<TSnapshot>(string snapshotData, string snapshotAssembly) where TSnapshot : class
        {
            object snapshot = Serializer.Deserialize(snapshotData, Type.GetType(snapshotAssembly));

            if (snapshot is null)
            {
                string eventTypeName = typeof(TSnapshot).FullName;
                string errorMessage = $"The {eventTypeName} could not be deserialized as a snapshot event. Check the Metadata property of this exception.";

                throw new EventDeserializationException(eventTypeName, snapshotData, ReservedErrorCode.SystemError, errorMessage);
            }

            return snapshot as TSnapshot;
        }
    }
}
