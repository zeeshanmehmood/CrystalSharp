using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public interface IEventStorePersistence
    {
        Task<IEnumerable<TEvent>> Get<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class;
        Task<TEvent> GetByVersion<TEvent>(string stream, long version, CancellationToken cancellationToken = default)
            where TEvent : class;
        Task<TEvent> GetLastEvent<TEvent>(string stream, CancellationToken cancellationToken = default)
            where TEvent : class;
        Task Store<TEvent>(string stream, IEnumerable<EventDataItem<TEvent>> eventsData,
            long expectedVersion,
            CancellationToken cancellationToken = default)
            where TEvent : class;
        Task Delete(string stream, CancellationToken cancellationToken = default);
        IEnumerable<EventDataItem<TEvent>> PrepareEventData<TEvent>(IEnumerable<TEvent> list,
            IDictionary<string, object> headers)
            where TEvent : class;
        string IdToStreamName(Type type, Guid id);
        long GetExpectedVersion(long originalVersion);
    }
}
