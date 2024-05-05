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
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public abstract class SqlEventStorePersistence : EventStorePersistence
    {
        private readonly IDbManager _dbManager;
        private readonly EventStoreQuery _eventStoreQuery;

        protected SqlEventStorePersistence(IDbManager dbManager, EventStoreQuery eventStoreQuery)
        {
            _dbManager = dbManager;
            _eventStoreQuery = eventStoreQuery;
        }

        protected abstract IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters);

        public async Task<IEnumerable<TEvent>> Get<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.GetEventQuery(stream);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);

            IEnumerable<TEvent> history = null;
            IEnumerable<IDbDomainEvent> events = await _dbManager.ExecuteQuery<DbDomainEvent>(query, parameters).ConfigureAwait(false);

            if (!events.HasAny())
            {
                ThrowStreamNotFoundException(stream);
            }

            history = events.Select(e => DeserializeEvent<TEvent>(e.Data, e.EventAssembly));

            return history;
        }

        public async Task<TEvent> GetByVersion<TEvent>(string stream, long version, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            TEvent @event = default;

            ValidateStreamVersion(stream, version);

            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.GetByVersionQuery(stream, version);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);
            IEnumerable<IDbDomainEvent> readResult = await _dbManager.ExecuteQuery<DbDomainEvent>(query, parameters).ConfigureAwait(false);

            if (!readResult.HasAny())
            {
                ThrowStreamNotFoundException(stream);
            }

            IDbDomainEvent dbDomainEvent = readResult.FirstOrDefault();
            @event = DeserializeEvent<TEvent>(dbDomainEvent.Data, dbDomainEvent.EventAssembly);

            return @event;
        }

        public async Task<TEvent> GetLastEvent<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            TEvent lastEvent = default;
            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.GetLastEventQuery(stream);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);
            IEnumerable<DbDomainEvent> readResult = await _dbManager.ExecuteQuery<DbDomainEvent>(query, parameters).ConfigureAwait(false);

            if (!readResult.HasAny())
            {
                ThrowStreamNotFoundException(stream);
            }

            IDbDomainEvent dbDomainEvent = readResult.LastOrDefault();
            lastEvent = DeserializeEvent<TEvent>(dbDomainEvent.Data, dbDomainEvent.EventAssembly);

            return lastEvent;
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
            long lastVersion = await GetLastVersion(stream).ConfigureAwait(false);

            if (lastVersion >= expectedVersion)
            {
                string errorMessage = $"The expected version {expectedVersion} already exists. Check the Stream, LastVersion and ExpectedVersion properties of this exception.";

                throw new EventStoreVersionConflictException(stream, lastVersion, expectedVersion, ReservedErrorCode.SystemError, errorMessage);
            }

            long lastSequence = await GetLastSequence(stream);

            foreach (IDbDomainEvent @event in eventsToSave)
            {
                (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.StoreEventQuery(Guid.NewGuid(),
                    @event.StreamId,
                    @event.StreamName,
                    ++lastSequence,
                    @event.EventId,
                    @event.EventType,
                    @event.EventAssembly,
                    @event.EntityStatus,
                    @event.CreatedOn,
                    @event.OccuredOn,
                    @event.Version,
                    @event.Data);
                IList<IDataParameter> parameters = GenerateParameters(dataParameters);

                await _dbManager.ExecuteNonQuery(query, parameters).ConfigureAwait(false);
            }
        }

        public async Task Delete(string stream, CancellationToken cancellationToken = default)
        {
            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.DeleteEventQuery(stream);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);

            await _dbManager.ExecuteNonQuery(query, parameters);
        }

        public override long GetExpectedVersion(long originalVersion)
        {
            return ++originalVersion;
        }

        private async Task<long> GetLastVersion(string stream)
        {
            long lastVersion = -1;
            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.GetLastVersionQuery(stream);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);
            object result = await _dbManager.ExecuteScalar(query, parameters).ConfigureAwait(false);

            if (result != null)
            {
                lastVersion = (long)result;
            }

            return lastVersion;
        }

        private async Task<long> GetLastSequence(string stream)
        {
            long lastSequence = 0;
            (string query, IDictionary<string, object> dataParameters) = _eventStoreQuery.GetLastEventSequence(stream);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);
            object result = await _dbManager.ExecuteScalar(query, parameters).ConfigureAwait(false);

            if (result != null)
            {
                lastSequence = (long)result;
            }

            return lastSequence;
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
