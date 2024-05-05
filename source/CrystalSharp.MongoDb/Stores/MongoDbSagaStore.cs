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

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using CrystalSharp.Common.Settings;
using CrystalSharp.Infrastructure;
using CrystalSharp.MongoDb.Database;
using CrystalSharp.Sagas;

namespace CrystalSharp.MongoDb.Stores
{
    public class MongoDbSagaStore : ISagaStore
    {
        private readonly IMongoDbSagaContext _mongoDbSagaContext;
        private readonly string _collection;

        public MongoDbSagaStore(IMongoDbSagaContext mongoDbSagaContext)
        {
            _mongoDbSagaContext = mongoDbSagaContext;
            _collection = ReservedTableName.SagaTransaction;
        }

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
                sagaTransactionMeta.CreatedOn = SystemDate.UtcNow;
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
