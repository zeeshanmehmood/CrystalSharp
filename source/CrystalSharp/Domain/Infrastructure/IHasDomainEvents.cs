using System.Collections.Generic;

namespace CrystalSharp.Domain.Infrastructure
{
    public interface IHasDomainEvents
    {
        IReadOnlyList<IDomainEvent> UncommittedEvents();
        void MarkEventsAsCommitted();
        int EventsCount();
        void Raise(IDomainEvent @event, long version);
    }
}
