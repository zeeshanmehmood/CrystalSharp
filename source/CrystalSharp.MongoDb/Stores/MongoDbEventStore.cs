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
using MongoDB.Driver.Linq;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions;
using CrystalSharp.MongoDb.Stores.Models;

namespace CrystalSharp.MongoDb.Stores
{
    public class MongoDbEventStore : EventStorePersistence, IEventStorePersistence
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<EventStoreData> _documentCollection;

        public MongoDbEventStore(string connectionString, string database)
        {
            _mongoClient = new MongoClient(connectionString);
            _mongoDatabase = _mongoClient.GetDatabase(database);
            _documentCollection = GetCollection<EventStoreData>();
        }

        public async Task<IEnumerable<TEvent>> Get<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            IEnumerable<TEvent> history = null;
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream));
            IEnumerable<IDbDomainEvent> events = _documentCollection.Find(filter).SortBy(x => x.Version).ToEnumerable();

            if (!events.HasAny())
            {
                ThrowStreamNotFoundException(stream);
            }

            history = events.Select(e => DeserializeEvent<TEvent>(e.Data, e.EventAssembly));

            return await Task.FromResult(history);
        }

        public async Task<TEvent> GetByVersion<TEvent>(string stream, long version, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            ValidateStreamVersion(stream, version);

            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream), PrepareVersionFilter(version));
            IDbDomainEvent dbDomainEvent = await _documentCollection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (dbDomainEvent == null)
            {
                ThrowStreamNotFoundException(stream);
            }

            TEvent @event = DeserializeEvent<TEvent>(dbDomainEvent.Data, dbDomainEvent.EventAssembly);

            return @event;
        }

        public async Task<TEvent> GetLastEvent<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream));
            IEnumerable<IDbDomainEvent> events = _documentCollection.Find(filter).SortBy(x => x.Version).ToEnumerable();

            if (!events.HasAny())
            {
                ThrowStreamNotFoundException(stream);
            }

            IDbDomainEvent dbDomainEvent = events.LastOrDefault();
            TEvent lastEvent = DeserializeEvent<TEvent>(dbDomainEvent.Data, dbDomainEvent.EventAssembly);

            return await Task.FromResult(lastEvent);
        }

        public async Task Store<TEvent>(string stream,
            IEnumerable<EventDataItem<TEvent>> eventsData,
            long expectedVersion,
            CancellationToken cancellationToken = default)
            where TEvent : class
        {
            if (!eventsData.HasAny())
            {
                string errorMessage = "There are no events to store. Check the Stream property of this exception.";

                throw new EventStoreZeroEventsException(stream, ReservedErrorCode.SystemError, errorMessage);
            }

            IEnumerable<IDbDomainEvent> eventsToSave = eventsData.Select(e => ToEventData(stream, e.Event));
            long lastVersion = GetLastVersion(stream);

            if (lastVersion >= expectedVersion)
            {
                string errorMessage = $"The expected version {expectedVersion} already exists. Check the Stream, LastVersion and ExpectedVersion properties of this exception.";

                throw new EventStoreVersionConflictException(stream, lastVersion, expectedVersion, ReservedErrorCode.SystemError, errorMessage);
            }

            long lastSequence = GetLastSequence(stream);

            foreach (IDbDomainEvent @event in eventsToSave)
            {
                string sequenceName = ReservedColumnName.GlobalSequence;
                GlobalDataSequence globalDataSequence = new();
                @event.Id = Guid.NewGuid();
                @event.GlobalSequence = globalDataSequence.GetNextSequenceValue(_mongoDatabase, sequenceName);
                @event.Sequence = ++lastSequence;
                EventStoreData record = @event.CopyTo<EventStoreData>();

                await _documentCollection.InsertOneAsync(record, null, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Delete(string stream, CancellationToken cancellationToken = default)
        {
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream));
            EventStoreData existingRecord = _documentCollection.Find(filter).FirstOrDefault();

            if (existingRecord != null)
            {
                existingRecord.EntityStatus = 0;

                await _documentCollection.FindOneAndReplaceAsync(filter, existingRecord, null, cancellationToken).ConfigureAwait(false);
            }
        }

        public override long GetExpectedVersion(long originalVersion)
        {
            return ++originalVersion;
        }

        private long GetLastVersion(string stream)
        {
            long lastVersion = -1;
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream));
            IDbDomainEvent dbDomainEvent = _documentCollection.Find(filter).SortByDescending(x => x.Version)
                .Limit(1)
                .FirstOrDefault();

            if (dbDomainEvent != null)
            {
                lastVersion = dbDomainEvent.Version;
            }

            return lastVersion;
        }

        private long GetLastSequence(string stream)
        {
            long lastSequence = 0;
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter
                .And(PrepareEntityStatusFilter(), PrepareStreamNameFilter(stream));
            IDbDomainEvent dbDomainEvent = _documentCollection.Find(filter).SortByDescending(x => x.Sequence)
                .Limit(1)
                .FirstOrDefault();

            if (dbDomainEvent != null)
            {
                lastSequence = dbDomainEvent.Sequence;
            }

            return lastSequence;
        }

        private FilterDefinition<EventStoreData> PrepareEntityStatusFilter()
        {
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter.Eq(x => x.EntityStatus, 1);

            return filter;
        }

        private FilterDefinition<EventStoreData> PrepareStreamNameFilter(string stream)
        {
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter.Eq(x => x.StreamName, stream);

            return filter;
        }

        private FilterDefinition<EventStoreData> PrepareVersionFilter(long version)
        {
            FilterDefinition<EventStoreData> filter = Builders<EventStoreData>.Filter.Eq(x => x.Version, version);

            return filter;
        }

        private IMongoCollection<T> GetCollection<T>() where T : class
        {
            string collection = typeof(T).Name;

            return _mongoDatabase.GetCollection<T>(collection);
        }

        private void ValidateStreamVersion(string stream, long version)
        {
            if (version < 0)
            {
                string errorMessage = "The stream version cannot be negative. Check the Stream and Version properties of this exception.";

                throw new EventStoreNegativeVersionException(stream, version, ReservedErrorCode.SystemError, errorMessage);
            }
        }

        private void ThrowStreamNotFoundException(string stream)
        {
            string errorMessage = "Stream not found. Check the Stream property of this exception.";

            throw new EventStoreStreamNotFoundException(stream, ReservedErrorCode.SystemError, errorMessage);
        }

        private IDbDomainEvent ToEventData<TEvent>(string stream, TEvent @event) where TEvent : class
        {
            IDbDomainEvent domainEvent = @event.CopyTo<DbDomainEvent>();

            if (domainEvent != null)
            {
                domainEvent.StreamName = stream;
                domainEvent.Data = Serializer.Serialize(@event);
            }

            return domainEvent;
        }

        private TEvent DeserializeEvent<TEvent>(string eventData, string eventAssembly) where TEvent : class
        {
            object @event = Serializer.Deserialize(eventData, Type.GetType(eventAssembly));

            if (@event is null)
            {
                string eventTypeName = typeof(TEvent).FullName;
                string errorMessage = $"The {eventTypeName} could not be deserialized as an event. Check the Metadata property of this exception.";

                throw new EventDeserializationException(eventTypeName, eventData, ReservedErrorCode.SystemError, errorMessage);
            }

            return @event as TEvent;
        }
    }
}
