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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure;

namespace CrystalSharp.Domain
{
    public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
    {
        private IList<IDomainEvent> Events { get; } = new List<IDomainEvent>();

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
                CreatedOn = @event.CreatedOn;
                ModifiedOn = @event.ModifiedOn;

                ApplyEvent(@event, false);
            }
        }

        protected void Raise(IDomainEvent @event)
        {
            SetCreatedOn(SystemDate.UtcNow);

            @event.StreamId = GlobalUId;
            @event.EventType = @event.GetType().Name;
            @event.EventAssembly = @event.GetType().AssemblyQualifiedName;
            @event.EntityStatus = (int)EntityStatus;
            @event.CreatedOn = CreatedOn;
            @event.ModifiedOn = ModifiedOn.HasValue ? ModifiedOn : default;
            @event.OccuredOn = SystemDate.UtcNow;

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
