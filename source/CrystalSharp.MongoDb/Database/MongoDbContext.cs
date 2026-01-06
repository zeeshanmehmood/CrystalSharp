using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MongoDb.Database
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IEventDispatcher _eventDispatcher;

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

        private async Task SaveDocument<TDocument>(TDocument document, CancellationToken cancellationToken = default)
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

            List<List<IDomainEvent>> eventsToDispatch = [];

            if (document is IHasDomainEvents entity && entity.EventsCount() > 0)
            {
                IReadOnlyList<IDomainEvent> domainEvents = entity.UncommittedEvents();

                eventsToDispatch.Add([.. domainEvents.Select(x => x)]);
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
                document.SetCreatedAt(SystemDate.UtcNow);
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
