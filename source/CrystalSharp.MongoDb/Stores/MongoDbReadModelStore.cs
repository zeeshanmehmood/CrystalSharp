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
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModels;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MongoDb.Extensions;

namespace CrystalSharp.MongoDb.Stores
{
    public class MongoDbReadModelStore : IReadModelStore<string>
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbReadModelStore(string connectionString, string database)
        {
            _mongoClient = new MongoClient(connectionString);
            _mongoDatabase = _mongoClient.GetDatabase(database);
        }

        public async Task<bool> Store<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            await SaveRecord(record, cancellationToken).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> BulkStore<T>(IEnumerable<T> records, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            return await BulkSaveRecord(records, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Update<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            await SaveRecord(record, cancellationToken).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> Delete<T>(string id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, string>(id);

            return await DeleteRecord<T, string>(idFilter, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Delete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, Guid>(globalUId);

            return await DeleteRecord<T, Guid>(idFilter, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> SoftDelete<T>(string id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, string>(id);

            return await SoftDeleteRecord<T, string>(idFilter, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> SoftDelete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, Guid>(globalUId);

            return await SoftDeleteRecord<T, Guid>(idFilter, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkDelete<T>(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            return await BulkDeleteRecords<T, string>(ids, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            return await BulkDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkSoftDelete<T>(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            return await BulkSoftDeleteRecords<T, string>(ids, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkSoftDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            return await BulkSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Restore<T>(string id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, string>(id);

            return await RestoreSoftDeletedRecord(idFilter, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Restore<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, Guid>(globalUId);

            return await RestoreSoftDeletedRecord(idFilter, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkRestore<T>(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            return await BulkRestoreSoftDeleteRecords<T, string>(ids, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkRestore<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            return await BulkRestoreSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);
        }

        public async Task<long> Count<T>(RecordMode recordMode = RecordMode.Active, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            IMongoCollection<T> documentCollection = GetCollection<T>();

            return await documentCollection.CountDocumentsAsync(GenerateEntityStatusFilter<T>(recordMode), null, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<long> Count<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            IMongoCollection<T> documentCollection = GetCollection<T>();
            long totalCount = await documentCollection.AsQueryable().Where(predicate).LongCountAsync(cancellationToken).ConfigureAwait(false);

            return totalCount;
        }

        public async Task<T> Find<T>(string id, bool tracking = false, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> filter = Builders<T>.Filter.And(GenerateIdFilter<T, string>(id), GenerateEntityStatusFilter<T>());

            return await FindRecord(filter, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> Find<T>(Guid globalUId, bool tracking = false, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> filter = Builders<T>.Filter.And(GenerateIdFilter<T, Guid>(globalUId), GenerateEntityStatusFilter<T>());

            return await FindRecord(filter, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IQueryable<T>> Filter<T>(Expression<Func<T, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            await Task.CompletedTask;

            IQueryable<T> records = GetCollection<T>().AsQueryable().Where(predicate);

            return records;
        }

        public async Task<PagedResult<T>> Get<T>(int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            bool tracking = false,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            await Task.CompletedTask;

            PagedResult<T> result = await GetRecords<T>(skip, take, predicate, recordMode, sortColumn, sortMode, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<PagedResult<T>> Search<T>(string term,
            bool useWildcard,
            int skip = 0,
            int take = 10,
            bool tracking = false,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            await Task.CompletedTask;

            throw new NotImplementedException("NOTE: Use \"Get\" method for search.");
        }

        private FilterDefinition<T> GenerateIdFilter<T, TId>(TId id)
        {
            return Builders<T>.Filter.Eq((id is Guid) ? ReservedColumnName.GlobalUId : ReservedColumnName._Id.ToLower(), id);
        }

        private FilterDefinition<T> GenerateEntityStatusFilter<T>(RecordMode recordMode = RecordMode.Active)
        {
            FilterDefinition<T> filter;

            if (recordMode == RecordMode.Active || recordMode == RecordMode.SoftDeleted)
            {
                EntityStatus entityStatus = (recordMode == RecordMode.Active) ? EntityStatus.Active : EntityStatus.Deleted;
                filter = Builders<T>.Filter.Eq(ReservedColumnName.EntityStatus, entityStatus);
            }
            else
            {
                filter = Builders<T>.Filter.Empty;
            }

            return filter;
        }

        private async Task SaveRecord<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> filter = Builders<T>.Filter.And(GenerateIdFilter<T, Guid>(record.GlobalUId), GenerateEntityStatusFilter<T>());
            IMongoCollection<T> documentCollection = GetCollection<T>();
            T existingRecord = documentCollection.Find(filter).FirstOrDefault();
            bool existing = existingRecord != null;

            DateTraction<T>(record, existing);

            if (existing)
            {
                await documentCollection.FindOneAndReplaceAsync(filter, record, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await documentCollection.InsertOneAsync(record, null, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<bool> BulkSaveRecord<T>(IEnumerable<T> records, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            if (!records.HasAny()) return false;

            IList<WriteModel<T>> recordsToStore = new List<WriteModel<T>>();

            foreach (T record in records)
            {
                DateTraction(record, false);
                recordsToStore.Add(new InsertOneModel<T>(record));
            }

            IMongoCollection<T> documentCollection = GetCollection<T>();
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToStore, null, cancellationToken).ConfigureAwait(false);
            bool bulkSaved = IsValidBulkWriteResult(result) && result.InsertedCount > 0;

            return bulkSaved;
        }

        private async Task<T> FindAndReplaceRecord<T>(FilterDefinition<T> filter, T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            record.ModifiedOn = SystemDate.UtcNow;
            IMongoCollection<T> documentCollection = GetCollection<T>();
            FindOneAndReplaceOptions<T> options = new() { ReturnDocument = ReturnDocument.After };

            return await documentCollection.FindOneAndReplaceAsync(filter, record, options, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> DeleteRecord<T, TId>(FilterDefinition<T> idFilter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> filter = Builders<T>.Filter.And(idFilter, GenerateEntityStatusFilter<T>());
            IMongoCollection<T> documentCollection = GetCollection<T>();
            T existingRecord = documentCollection.Find(filter).FirstOrDefault();

            if (existingRecord == null) return false;

            DeleteResult result = await documentCollection.DeleteOneAsync(filter, cancellationToken).ConfigureAwait(false);
            bool deleted = result != null && result.IsAcknowledged && result.DeletedCount > 0;

            return deleted;
        }

        private async Task<bool> SoftDeleteRecord<T, TId>(FilterDefinition<T> idFilter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> filter = Builders<T>.Filter.And(idFilter, GenerateEntityStatusFilter<T>());
            T existingRecord = await FindRecord(filter, cancellationToken).ConfigureAwait(false);

            if (existingRecord == null) return false;

            existingRecord.EntityStatus = EntityStatus.Deleted;
            T record = await FindAndReplaceRecord(filter, existingRecord, cancellationToken).ConfigureAwait(false);

            return record.EntityStatus == EntityStatus.Deleted;
        }

        private async Task<bool> BulkDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            if (!ids.HasAny()) return false;

            FilterDefinition<T> idsFilter = Builders<T>.Filter.In((ids is IEnumerable<Guid>)
                ?
                ReservedColumnName.GlobalUId
                :
                ReservedColumnName._Id.ToLower(), ids);
            FilterDefinition<T> filter = Builders<T>.Filter.And(idsFilter, GenerateEntityStatusFilter<T>());
            IMongoCollection<T> documentCollection = GetCollection<T>();
            IList<WriteModel<T>> recordsToDelete = new List<WriteModel<T>>() { new DeleteManyModel<T>(filter) };
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToDelete, null, cancellationToken).ConfigureAwait(false);
            bool bulkDeleted = IsValidBulkWriteResult(result) && result.DeletedCount > 0;

            return bulkDeleted;
        }

        private async Task<bool> BulkSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            if (!ids.HasAny()) return false;

            FilterDefinition<T> idsFilter = Builders<T>.Filter.In((ids is IEnumerable<Guid>)
                ?
                ReservedColumnName.GlobalUId
                :
                ReservedColumnName._Id.ToLower(), ids);
            FilterDefinition<T> filter = Builders<T>.Filter.And(idsFilter, GenerateEntityStatusFilter<T>());
            UpdateDefinition<T> updateDefinition = Builders<T>.Update
                .Set(x => x.EntityStatus, EntityStatus.Deleted)
                .Set(y => y.ModifiedOn, SystemDate.UtcNow);
            IList<WriteModel<T>> recordsToDelete = new List<WriteModel<T>>() { new UpdateManyModel<T>(filter, updateDefinition) };
            IMongoCollection<T> documentCollection = GetCollection<T>();
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToDelete, null, cancellationToken).ConfigureAwait(false);
            bool bulkDeleted = IsValidBulkWriteResult(result) && result.ModifiedCount > 0;

            return bulkDeleted;
        }

        private async Task<bool> RestoreSoftDeletedRecord<T>(FilterDefinition<T> idFilter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            T existingRecord = await FindRecord(idFilter, cancellationToken).ConfigureAwait(false);

            if (existingRecord == null) return false;

            existingRecord.EntityStatus = EntityStatus.Active;
            T record = await FindAndReplaceRecord(idFilter, existingRecord, cancellationToken).ConfigureAwait(false);

            return record.EntityStatus == EntityStatus.Active;
        }

        private async Task<bool> BulkRestoreSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            if (!ids.HasAny()) return false;

            FilterDefinition<T> idsFilter = Builders<T>.Filter.In((ids is IEnumerable<Guid>)
                ?
                ReservedColumnName.GlobalUId
                :
                ReservedColumnName._Id.ToLower(), ids);
            FilterDefinition<T> filter = Builders<T>.Filter.And(idsFilter, GenerateEntityStatusFilter<T>(RecordMode.SoftDeleted));
            UpdateDefinition<T> updateDefinition = Builders<T>.Update
                .Set(x => x.EntityStatus, EntityStatus.Active)
                .Set(y => y.ModifiedOn, SystemDate.UtcNow);
            IList<WriteModel<T>> recordsToRestore = new List<WriteModel<T>>() { new UpdateManyModel<T>(filter, updateDefinition) };
            IMongoCollection<T> documentCollection = GetCollection<T>();
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToRestore, null, cancellationToken).ConfigureAwait(false);
            bool bulkRestored = IsValidBulkWriteResult(result) && result.ModifiedCount > 0;

            return bulkRestored;
        }

        private async Task<T> FindRecord<T>(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            IMongoCollection<T> documentCollection = GetCollection<T>();

            return await documentCollection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<PagedResult<T>> GetRecords<T>(int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            await Task.CompletedTask;

            long totalRecords = 0;
            Expression<Func<T, bool>> recordModePredicate;

            if (predicate == null)
            {
                predicate = x => true;
            }

            if (recordMode == RecordMode.Active)
            {
                recordModePredicate = x => x.EntityStatus == EntityStatus.Active;
            }
            else if (recordMode == RecordMode.SoftDeleted)
            {
                recordModePredicate = x => x.EntityStatus == EntityStatus.Deleted;
            }
            else
            {
                recordModePredicate = x => true;
            }

            IMongoCollection<T> documentCollection = GetCollection<T>();
            totalRecords = documentCollection.AsQueryable().Where(predicate).Where(recordModePredicate).LongCount();
            IQueryable<T> records = documentCollection.Get<T, string>(skip, take, predicate, recordMode, sortColumn, sortMode);
            PagedResult<T> result = null;

            if (records.HasAny())
            {
                result = new PagedResult<T>(skip, take, totalRecords, records);
            }

            return result;
        }

        private bool IsValidBulkWriteResult<T>(BulkWriteResult<T> bulkWriteResult)
            where T : class, IReadModel<string>
        {
            return bulkWriteResult != null && bulkWriteResult.IsAcknowledged;
        }

        private void DateTraction<T>(T record, bool existing) where T : IReadModel<string>
        {
            if (!existing)
            {
                record.CreatedOn = SystemDate.UtcNow;
            }
            else
            {
                record.ModifiedOn = SystemDate.UtcNow;
            }
        }

        private IMongoCollection<T> GetCollection<T>() where T : class, IReadModel<string>
        {
            string collection = typeof(T).Name;

            return _mongoDatabase.GetCollection<T>(collection);
        }
    }
}
