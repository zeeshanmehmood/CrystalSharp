using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CrystalSharp.Domain
{
    public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
    {
        private List<IDomainEvent> Events { get; } = [];

        public long Version { get; private set; } = -1;

        public IReadOnlyList<IDomainEvent> UncommittedEvents()
        {
            return new ReadOnlyCollection<IDomainEvent>(Events);
        }

        public void MarkEventsAsCommitted()
        {
            Events.Clear();
        }

        public int EventsCount()
        {
            return Events.Count;
        }

        void IHasDomainEvents.Raise(IDomainEvent @event, long version)
        {
            if (!Events.Any(x => x.EventId == @event.EventId))
            {
                Version = version;
                @event.Version = version;
            }

            Events.Add(@event);
        }

        public void ThrowDomainException(string message)
        {
            throw new DomainException(message);
        }

        public void ThrowDomainException(int errorCode, string message)
        {
            throw new DomainException(errorCode, message);
        }

        public void LoadStateFromHistory(IEnumerable<IDomainEvent> events)
        {
            foreach (IDomainEvent @event in events)
            {
                GlobalUId = @event.StreamId;
                EntityStatus = (EntityStatus)@event.EntityStatus;
                CreatedAt = @event.CreatedAt;
                ModifiedOn = @event.ModifiedOn;

                ApplyEvent(@event, false);
            }
        }

        protected void Raise(IDomainEvent @event)
        {
            SetCreatedAt(SystemDate.UtcNow);

            @event.StreamId = GlobalUId;
            @event.EventType = @event.GetType().Name;
            @event.EventAssembly = @event.GetType().AssemblyQualifiedName;
            @event.EntityStatus = (int)EntityStatus;
            @event.CreatedAt = CreatedAt;
            @event.ModifiedOn = ModifiedOn.HasValue ? ModifiedOn : default;
            @event.OccurredOn = SystemDate.UtcNow;

            (this as IHasDomainEvents).Raise(@event, Version + 1);
        }

        protected internal void ApplyEvent(IDomainEvent @event)
        {
            ApplyEvent(@event, true);
        }

        protected virtual void ApplyEvent(IDomainEvent @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);

            if (isNew)
            {
                @event.Version = ++Version;

                Events.Add(@event);
            }
            else
            {
                Version = @event.Version;
            }
        }
    }
}
