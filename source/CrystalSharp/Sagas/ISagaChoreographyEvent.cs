using CrystalSharp.Domain;
using CrystalSharp.Domain.Infrastructure;

namespace CrystalSharp.Sagas
{
    public interface ISagaChoreographyEvent<TAggregateRoot, TKey, TDomainEvent> : ISynchronousDomainEventHandler<TDomainEvent>
        where TAggregateRoot : IAggregateRoot<TKey>
        where TDomainEvent : IDomainEvent
    {
        //
    }
}
