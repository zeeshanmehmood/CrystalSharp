using CrystalSharp.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public interface IAggregateEventStore<TKey>
    {
        Task<TAggregate> Get<TAggregate>(Guid streamId, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>;
        Task<TAggregate> GetByVersion<TAggregate>(Guid streamId, long version, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>;
        Task<TAggregate> GetByVersion<TAggregate>(Guid streamId, long version, TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>;
        Task Store<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>;
        Task Delete<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>;
        Task Delete<TAggregate>(Guid streamId, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>;
    }
}
