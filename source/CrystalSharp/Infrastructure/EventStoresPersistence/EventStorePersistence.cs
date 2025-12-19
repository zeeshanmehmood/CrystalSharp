using CrystalSharp.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public abstract class EventStorePersistence
    {
        public abstract long GetExpectedVersion(long originalVersion);

        public string IdToStreamName(Type type, Guid id)
        {
            return type.ToStreamName(id);
        }

        public IEnumerable<EventDataItem<TEvent>> PrepareEventData<TEvent>(IEnumerable<TEvent> list,
            IDictionary<string, object> headers)
            where TEvent : class
        {
            IEnumerable<EventDataItem<TEvent>> eventsToSave = null;

            if (list.HasAny())
            {
                eventsToSave = list.Select(e => new EventDataItem<TEvent> { Headers = headers, Event = e });
            }

            return eventsToSave;
        }
    }
}
