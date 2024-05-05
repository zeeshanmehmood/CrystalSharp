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
using System.Text;
using EventStore.Client;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions;

namespace CrystalSharp.EventStores.EventStoreDb.Stores
{
    public class EventStoreDbPersistence : EventStorePersistence, IEventStorePersistence
    {
        private readonly EventStoreClient _eventStore;
        private const string EVENT_CLR_TYPE_HEADER = ReservedName.EventClrTypeName;

        public EventStoreDbPersistence(EventStoreClient eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<IEnumerable<TEvent>> Get<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            return await ReadAllEvents<TEvent>(stream, long.MaxValue, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEvent> GetByVersion<TEvent>(string stream, long version, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            ValidateStreamVersion(stream, version);

            EventStoreClient.ReadStreamResult readResult = _eventStore.ReadStreamAsync(Direction.Forwards,
                stream,
                StreamPosition.FromStreamRevision(StreamRevision.FromInt64(version)),
                cancellationToken: cancellationToken);

            if (await readResult.ReadState == ReadState.StreamNotFound)
            {
                ThrowStreamNotFoundException(stream);
            }

            ResolvedEvent resolvedEvent = await readResult.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            return DeserializeResolvedEvent<TEvent>(resolvedEvent);
        }

        public async Task<TEvent> GetLastEvent<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            EventStoreClient.ReadStreamResult readResult = _eventStore.ReadStreamAsync(Direction.Backwards,
                stream,
                StreamPosition.End,
                cancellationToken: cancellationToken);

            if (await readResult.ReadState == ReadState.StreamNotFound)
            {
                ThrowStreamNotFoundException(stream);
            }

            ResolvedEvent resolvedEvent = await readResult.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            return DeserializeResolvedEvent<TEvent>(resolvedEvent);
        }

        public async Task Store<TEvent>(string stream, IEnumerable<EventDataItem<TEvent>> eventsData,
            long expectedVersion,
            CancellationToken cancellationToken = default)
            where TEvent : class
        {
            if (!eventsData.HasAny())
            {
                string errorMessage = "There are no events to store. Check the Stream property of this exception.";

                throw new EventStoreZeroEventsException(stream, ReservedErrorCode.SystemError, errorMessage);
            }

            IEnumerable<EventData> eventsToSave = eventsData.Select(e => ToEventData(e.Event, e.Headers));

            if (eventsToSave.HasAny())
            {
                await _eventStore.AppendToStreamAsync(stream,
                    GetExpectedStream(expectedVersion),
                    eventsToSave,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task Delete(string stream, CancellationToken cancellationToken = default)
        {
            await _eventStore.DeleteAsync(stream, StreamState.StreamExists, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public override long GetExpectedVersion(long originalVersion)
        {
            return originalVersion;
        }

        private async Task<IEnumerable<TEvent>> ReadAllEvents<TEvent>(string stream,
            long version,
            CancellationToken cancellationToken = default)
            where TEvent : class
        {
            ValidateStreamVersion(stream, version);

            EventStoreClient.ReadStreamResult readResult = _eventStore.ReadStreamAsync(Direction.Forwards,
                stream,
                StreamPosition.Start,
                cancellationToken: cancellationToken);
            IList<TEvent> history = new List<TEvent>();

            if (await readResult.ReadState == ReadState.StreamNotFound)
            {
                ThrowStreamNotFoundException(stream);
            }

            await foreach (ResolvedEvent resolvedEvent in readResult)
            {
                history.Add(DeserializeResolvedEvent<TEvent>(resolvedEvent));
            }

            if (!history.HasAny())
            {
                history = null;
            }

            return history;
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

        private StreamState GetExpectedStream(long originalVersion)
        {
            return originalVersion == -1 ? StreamState.Any : StreamState.StreamExists;
        }

        private TEvent DeserializeResolvedEvent<TEvent>(ResolvedEvent resolvedEvent)
            where TEvent : class
        {
            IDictionary<string, object> metadata = DeserializeMetadata(resolvedEvent.Event.Metadata.Span);
            string eventType = metadata[EVENT_CLR_TYPE_HEADER.ToCamelCase()].ToString();

            return DeserializeEvent<TEvent>(eventType, resolvedEvent.Event.Data.Span);
        }

        private EventData ToEventData<TEvent>(TEvent @event, IDictionary<string, object> headers)
            where TEvent : class
        {
            byte[] data = Encoding.UTF8.GetBytes(Serializer.Serialize(@event));
            IDictionary<string, object> eventHeaders = new Dictionary<string, object>(headers)
            {
                {
                    EVENT_CLR_TYPE_HEADER, @event.GetType().AssemblyQualifiedName
                }
            };

            byte[] metadata = Encoding.UTF8.GetBytes(Serializer.Serialize(eventHeaders));
            string typeName = @event.GetType().Name;

            return new EventData(Uuid.NewUuid(), typeName, data.AsMemory(), metadata.AsMemory());
        }

        private IDictionary<string, object> DeserializeMetadata(ReadOnlySpan<byte> data)
        {
            string json = ByteToJson(data);
            IDictionary<string, object> metadata = Serializer.Deserialize<IDictionary<string, object>>(json);

            if (metadata is null || metadata.Count == 0)
            {
                string errorMessage = "The metadata cannot be deserialized. Check the Metadata property of this exception.";

                throw new EventStoreMetadataDeserializationException(json, ReservedErrorCode.SystemError, errorMessage);
            }

            return metadata;
        }

        private TEvent DeserializeEvent<TEvent>(string eventType, ReadOnlySpan<byte> data)
            where TEvent : class
        {
            string json = ByteToJson(data);
            object @event = Serializer.Deserialize(json, Type.GetType(eventType));

            if (@event as TEvent == null)
            {
                string eventTypeName = typeof(TEvent).FullName;
                string errorMessage = $"The {eventTypeName} could not be deserialized as an event. Check the Payload property of this exception.";

                throw new EventStoreEventDeserializationException(eventTypeName, json, ReservedErrorCode.SystemError, errorMessage);
            }

            return @event as TEvent;
        }

        private string ByteToJson(ReadOnlySpan<byte> data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
