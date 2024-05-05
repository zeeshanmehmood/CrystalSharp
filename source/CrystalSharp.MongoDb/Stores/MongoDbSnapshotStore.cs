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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions;
using CrystalSharp.MongoDb.Stores.Models;

namespace CrystalSharp.MongoDb.Stores
{
    public class MongoDbSnapshotStore : EventStorePersistence, ISnapshotStore
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<SnapshotStoreData> _documentCollection;

        public MongoDbSnapshotStore(string connectionString, string database)
        {
            _mongoClient = new MongoClient(connectionString);
            _mongoDatabase = _mongoClient.GetDatabase(database);
            _documentCollection = GetCollection<SnapshotStoreData>();
        }

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
            long lastVersion = GetLastVersion(stream);

            if (lastVersion >= expectedVersion)
            {
                string errorMessage = $"The expected version {expectedVersion} of the snapshot already exists. Check the Stream, LastVersion and ExpectedVersion properties of this exception.";
                
                throw new SnapshotVersionConflictException(stream, lastVersion, expectedVersion, ReservedErrorCode.SystemError, errorMessage);
            }

            DbSnapshotEntity dbSnapshotEntity = new()
            {
                SnapshotId = Guid.NewGuid(),
                SnapshotAssembly = snapshot.GetType().AssemblyQualifiedName,
                SnapshotVersion = snapshot.SnapshotVersion,
                StreamName = stream,
                EntityStatus = 1,
                CreatedOn = SystemDate.UtcNow,
                Data = Serializer.Serialize(snapshot)
            };
            SnapshotStoreData record = dbSnapshotEntity.CopyTo<SnapshotStoreData>();

            await _documentCollection.InsertOneAsync(record, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TSnapshot> LoadSnapshot<TSnapshot>(Guid aggregateGlobalUId, CancellationToken cancellationToken = default)
            where TSnapshot : class, ISnapshot
        {
            string stream = IdToStreamName(typeof(TSnapshot), aggregateGlobalUId);
            TSnapshot snapshot = default;
            FilterDefinition<SnapshotStoreData> filter = Builders<SnapshotStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream));
            IEnumerable<IDbSnapshotEntity> snapshots = _documentCollection.Find(filter).SortByDescending(x => x.SnapshotVersion).ToEnumerable();

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

            return await Task.FromResult(snapshot);
        }

        public override long GetExpectedVersion(long originalVersion)
        {
            return ++originalVersion;
        }

        private long GetLastVersion(string stream)
        {
            long lastVersion = -1;
            FilterDefinition<SnapshotStoreData> filter = Builders<SnapshotStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream));
            IDbSnapshotEntity dbSnapshotEntity = _documentCollection.Find(filter).SortByDescending(x => x.SnapshotVersion)
                .Limit(1)
                .FirstOrDefault();

            if (dbSnapshotEntity != null)
            {
                lastVersion = dbSnapshotEntity.SnapshotVersion;
            }

            return lastVersion;
        }

        private FilterDefinition<SnapshotStoreData> PrepareEntityStatusFilter()
        {
            FilterDefinition<SnapshotStoreData> filter = Builders<SnapshotStoreData>.Filter.Eq(x => x.EntityStatus, 1);

            return filter;
        }

        private FilterDefinition<SnapshotStoreData> PrepareStreamNameFilter(string stream)
        {
            FilterDefinition<SnapshotStoreData> filter = Builders<SnapshotStoreData>.Filter.Eq(x => x.StreamName, stream);

            return filter;
        }

        private IMongoCollection<T> GetCollection<T>() where T : class
        {
            string collection = typeof(T).Name;

            return _mongoDatabase.GetCollection<T>(collection);
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
