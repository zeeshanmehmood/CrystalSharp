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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure;

namespace CrystalSharp.RavenDb.Database
{
    public class RavenDbContext : IRavenDbContext
    {
        private readonly IDocumentSession _session;
        private readonly IEventDispatcher _eventDispatcher;

        private List<List<IDomainEvent>> _eventsToDispatch;

        public IDocumentStore DocumentStore { get { return _session.Advanced.DocumentStore; } }
        public IDocumentSession Session { get { return _session; } }

        public RavenDbContext(IDocumentSession session, IEventDispatcher eventDispatcher)
        {
            _session = session;
            _eventDispatcher = eventDispatcher;

            _session.Advanced.OnBeforeStore += OnBeforeSave;
        }

        public async Task SaveChanges(CancellationToken cancellationToken = default)
        {
            _eventsToDispatch = new List<List<IDomainEvent>>();

            _session.SaveChanges();

            if (_eventsToDispatch.HasAny())
            {
                foreach (List<IDomainEvent> events in _eventsToDispatch)
                {
                    await _eventDispatcher.Dispatch(events.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                }

                _eventsToDispatch.Clear();
            }
        }

        private void OnBeforeSave(object sender, BeforeStoreEventArgs eventArgs)
        {
            ProcessDocument(eventArgs);
        }

        private void ProcessDocument(BeforeStoreEventArgs eventArgs)
        {
            if (eventArgs.Entity is ICreatedOnTrackedEntity && eventArgs.Entity is IModifiedOnTrackedEntity)
            {
                DateTraction(eventArgs.Entity as Entity<string>);
            }

            if (eventArgs.Entity is IHasSecondaryId)
            {
                AggregateRoot<string> document = eventArgs.Entity as AggregateRoot<string>;
                IReadOnlyList<IDomainEvent> domainEvents = document.UncommittedEvents().Select(x => x).ToList();

                _eventsToDispatch.Add(domainEvents.Select(x => x).ToList());

                document.MarkEventsAsCommitted();
            }
        }

        private void DateTraction(Entity<string> document)
        {
            if (document.CreatedOn == default)
            {
                document.SetCreatedOn(SystemDate.UtcNow);
            }
            else
            {
                document.SetModifiedOn(SystemDate.UtcNow);
            }
        }
    }
}
