using CrystalSharp.Domain.Infrastructure;
using System.Collections.Generic;

namespace CrystalSharp.Domain
{
    public interface IAggregateRoot<TKey> : IEntity<TKey>, IVersion, IHasDomainEvents
    {
        void LoadStateFromHistory(IEnumerable<IDomainEvent> events);
    }
}
