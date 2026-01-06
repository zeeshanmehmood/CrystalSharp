using CrystalSharp.Common.Settings;
using CrystalSharp.Infrastructure;
using CrystalSharp.MongoDb.Database;
using CrystalSharp.Sagas;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MongoDb.Stores
{
    public class MongoDbSagaStore(IMongoDbSagaContext mongoDbSagaContext) : ISagaStore
    {
        private readonly IMongoDbSagaContext _mongoDbSagaContext = mongoDbSagaContext;
        private readonly string _collection = ReservedTableName.SagaTransaction;

        public async Task<SagaTransactionMeta> Get(string correlationId, CancellationToken cancellationToken = default)
        {
            IMongoCollection<SagaTransactionMeta> documentCollection = GetCollection(_collection);
            FilterDefinition<SagaTransactionMeta> filter = PrepareCorrelationIdFilter(correlationId);

            return await documentCollection.Find(filter).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task Upsert(SagaTransactionMeta sagaTransactionMeta, CancellationToken cancellationToken = default)
        {
            IMongoCollection<SagaTransactionMeta> documentCollection = GetCollection(_collection);
            FilterDefinition<SagaTransactionMeta> filter = PrepareCorrelationIdFilter(sagaTransactionMeta.CorrelationId);
            SagaTransactionMeta existingDocument = await documentCollection.Find(filter).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            bool existing = existingDocument != null;

            if (!existing)
            {
                sagaTransactionMeta.CreatedAt = SystemDate.UtcNow;
            }
            else
            {
                sagaTransactionMeta.ModifiedOn = SystemDate.UtcNow;
            }

            if (existing)
            {
                await documentCollection.FindOneAndReplaceAsync(filter, sagaTransactionMeta, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await documentCollection.InsertOneAsync(sagaTransactionMeta, null, cancellationToken).ConfigureAwait(false);
            }
        }

        private FilterDefinition<SagaTransactionMeta> PrepareCorrelationIdFilter(string correlationId)
        {
            FilterDefinition<SagaTransactionMeta> filter = Builders<SagaTransactionMeta>.Filter.Eq(x => x.CorrelationId, correlationId);

            return filter;
        }

        private IMongoCollection<SagaTransactionMeta> GetCollection(string collection)
        {
            return _mongoDbSagaContext.GetDbContext().GetCollection<SagaTransactionMeta>(collection);
        }
    }
}
