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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure;

namespace CrystalSharp.MongoDb.Database
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbContext(string connectionString, string database, IEventDispatcher eventDispatcher)
        {
            _mongoClient = new MongoClient(connectionString);
            _mongoDatabase = _mongoClient.GetDatabase(database);
            _eventDispatcher = eventDispatcher;
        }

        public async Task SaveChanges<TDocument>(TDocument document, CancellationToken cancellationToken = default)
            where TDocument : IAggregateRoot<string>
        {
            await SaveDocument(document, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TDocument> Find<TDocument>(Guid globalUId, CancellationToken cancellationToken = default)
            where TDocument : IAggregateRoot<string>
        {
            TDocument document = await GetCollection<TDocument>()
                .AsQueryable()
                .SingleOrDefaultAsync(x => x.GlobalUId == globalUId, cancellationToken)
                .ConfigureAwait(false);

            return document;
        }

        public IQueryable<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> predicate)
            where TDocument : IAggregateRoot<string>
        {
            IQueryable<TDocument> documents = GetCollection<TDocument>().AsQueryable().Where(predicate);

            return documents;
        }

        private async Task SaveDocument<TDocument>(TDocument document,CancellationToken cancellationToken = default)
            where TDocument : IAggregateRoot<string>
        {
            FilterDefinition<TDocument> filter = Builders<TDocument>.Filter.Eq(ReservedColumnName.GlobalUId, document.GlobalUId);
            IMongoCollection<TDocument> documentCollection = GetCollection<TDocument>();
            TDocument existingDocument = documentCollection.Find(filter).FirstOrDefault();
            bool existing = existingDocument != null;

            DateTraction(document, existing);

            if (existing)
            {
                await documentCollection.FindOneAndReplaceAsync(filter, document, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await documentCollection.InsertOneAsync(document, null, cancellationToken).ConfigureAwait(false);
            }

            List<List<IDomainEvent>> eventsToDispatch = new();

            if (document is IHasDomainEvents entity && entity.EventsCount() > 0)
            {
                IReadOnlyList<IDomainEvent> domainEvents = entity.UncommittedEvents();

                eventsToDispatch.Add(domainEvents.Select(x => x).ToList());
                document.MarkEventsAsCommitted();
            }

            if (eventsToDispatch.HasAny())
            {
                foreach (List<IDomainEvent> events in eventsToDispatch)
                {
                    await _eventDispatcher.Dispatch(events.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void DateTraction(IEntity<string> document, bool existing)
        {
            if (!existing)
            {
                document.SetCreatedOn(SystemDate.UtcNow);
            }
            else
            {
                document.SetModifiedOn(SystemDate.UtcNow);
            }
        }

        private IMongoCollection<TDocument> GetCollection<TDocument>() where TDocument : IEntity<string>
        {
            return _mongoDatabase.GetCollection<TDocument>(typeof(TDocument).Name);
        }
    }
}
