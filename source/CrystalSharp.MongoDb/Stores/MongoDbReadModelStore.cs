using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModels;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MongoDb.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MongoDb.Stores
{
    public class MongoDbReadModelStore : IReadModelStore<string>
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbReadModelStore(string connectionString, string database)
        {
            _mongoClient = new MongoClient(connectionString);
            _mongoDatabase = _mongoClient.GetDatabase(database);
        }

        public async Task<int> Store<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;

            await SaveRecord(record, cancellationToken).ConfigureAwait(false);

            ++affected;

            return affected;
        }

        public async Task<int> BulkStore<T>(IEnumerable<T> records, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = await BulkSaveRecord(records, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> Update<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;

            await SaveRecord(record, cancellationToken).ConfigureAwait(false);

            ++affected;

            return affected;
        }

        public async Task<int> Delete<T>(string id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, string>(id);
            int affected = await DeleteRecord<T, string>(idFilter, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> Delete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, Guid>(globalUId);
            int affected = await DeleteRecord<T, Guid>(idFilter, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> SoftDelete<T>(string id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, string>(id);
            int affected = await SoftDeleteRecord<T, string>(idFilter, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> SoftDelete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, Guid>(globalUId);

            int affected = await SoftDeleteRecord<T, Guid>(idFilter, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> BulkDelete<T>(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = await BulkDeleteRecords<T, string>(ids, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> BulkDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = await BulkDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> BulkSoftDelete<T>(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = await BulkSoftDeleteRecords<T, string>(ids, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> BulkSoftDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = await BulkSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> Restore<T>(string id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, string>(id);
            int affected = await RestoreSoftDeletedRecord(idFilter, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> Restore<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            FilterDefinition<T> idFilter = GenerateIdFilter<T, Guid>(globalUId);
            int affected = await RestoreSoftDeletedRecord(idFilter, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> BulkRestore<T>(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = await BulkRestoreSoftDeleteRecords<T, string>(ids, cancellationToken).ConfigureAwait(false);

            return affected;
        }

        public async Task<int> BulkRestore<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = await BulkRestoreSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);

            return affected;
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

        private async Task<int> BulkSaveRecord<T>(IEnumerable<T> records, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;

            if (!records.HasAny()) return affected;

            IList<WriteModel<T>> recordsToStore = [];

            foreach (T record in records)
            {
                DateTraction(record, false);
                recordsToStore.Add(new InsertOneModel<T>(record));
            }

            IMongoCollection<T> documentCollection = GetCollection<T>();
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToStore, null, cancellationToken).ConfigureAwait(false);
            bool bulkSaved = IsValidBulkWriteResult(result) && result.InsertedCount > 0;

            if (bulkSaved)
            {
                affected = (int)result.InsertedCount;
            }

            return affected;
        }

        private async Task<T> FindAndReplaceRecord<T>(FilterDefinition<T> filter, T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            record.ModifiedOn = SystemDate.UtcNow;
            IMongoCollection<T> documentCollection = GetCollection<T>();
            FindOneAndReplaceOptions<T> options = new() { ReturnDocument = ReturnDocument.After };

            return await documentCollection.FindOneAndReplaceAsync(filter, record, options, cancellationToken).ConfigureAwait(false);
        }

        private async Task<int> DeleteRecord<T, TId>(FilterDefinition<T> idFilter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;
            FilterDefinition<T> filter = Builders<T>.Filter.And(idFilter, GenerateEntityStatusFilter<T>());
            IMongoCollection<T> documentCollection = GetCollection<T>();
            T existingRecord = documentCollection.Find(filter).FirstOrDefault();

            if (existingRecord is null) return affected;

            DeleteResult result = await documentCollection.DeleteOneAsync(filter, cancellationToken).ConfigureAwait(false);
            bool deleted = result != null && result.IsAcknowledged && result.DeletedCount > 0;

            if (deleted)
            {
                ++affected;
            }

            return affected;
        }

        private async Task<int> SoftDeleteRecord<T, TId>(FilterDefinition<T> idFilter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;
            FilterDefinition<T> filter = Builders<T>.Filter.And(idFilter, GenerateEntityStatusFilter<T>());
            T existingRecord = await FindRecord(filter, cancellationToken).ConfigureAwait(false);

            if (existingRecord is null) return affected;

            existingRecord.EntityStatus = EntityStatus.Deleted;
            T record = await FindAndReplaceRecord(filter, existingRecord, cancellationToken).ConfigureAwait(false);

            if (record.EntityStatus == EntityStatus.Deleted)
            {
                ++affected;
            }

            return affected;
        }

        private async Task<int> BulkDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;

            if (!ids.HasAny()) return affected;

            FilterDefinition<T> idsFilter = Builders<T>.Filter.In((ids is IEnumerable<Guid>)
                ?
                ReservedColumnName.GlobalUId
                :
                ReservedColumnName._Id.ToLower(), ids);
            FilterDefinition<T> filter = Builders<T>.Filter.And(idsFilter, GenerateEntityStatusFilter<T>());
            IMongoCollection<T> documentCollection = GetCollection<T>();
            IList<WriteModel<T>> recordsToDelete = [new DeleteManyModel<T>(filter)];
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToDelete, null, cancellationToken).ConfigureAwait(false);
            bool bulkDeleted = IsValidBulkWriteResult(result) && result.DeletedCount > 0;

            if (bulkDeleted)
            {
                affected = (int)result.DeletedCount;
            }

            return affected;
        }

        private async Task<int> BulkSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;

            if (!ids.HasAny()) return affected;

            FilterDefinition<T> idsFilter = Builders<T>.Filter.In((ids is IEnumerable<Guid>)
                ?
                ReservedColumnName.GlobalUId
                :
                ReservedColumnName._Id.ToLower(), ids);
            FilterDefinition<T> filter = Builders<T>.Filter.And(idsFilter, GenerateEntityStatusFilter<T>());
            UpdateDefinition<T> updateDefinition = Builders<T>.Update
                .Set(x => x.EntityStatus, EntityStatus.Deleted)
                .Set(y => y.ModifiedOn, SystemDate.UtcNow);
            IList<WriteModel<T>> recordsToDelete = [new UpdateManyModel<T>(filter, updateDefinition)];
            IMongoCollection<T> documentCollection = GetCollection<T>();
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToDelete, null, cancellationToken).ConfigureAwait(false);
            bool bulkDeleted = IsValidBulkWriteResult(result) && result.ModifiedCount > 0;

            if (bulkDeleted)
            {
                affected = (int)result.ModifiedCount;
            }

            return affected;
        }

        private async Task<int> RestoreSoftDeletedRecord<T>(FilterDefinition<T> idFilter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;
            T existingRecord = await FindRecord(idFilter, cancellationToken).ConfigureAwait(false);

            if (existingRecord is null) return affected;

            existingRecord.EntityStatus = EntityStatus.Active;
            T record = await FindAndReplaceRecord(idFilter, existingRecord, cancellationToken).ConfigureAwait(false);

            if (record.EntityStatus == EntityStatus.Active)
            {
                ++affected;
            }

            return affected;
        }

        private async Task<int> BulkRestoreSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            int affected = 0;

            if (!ids.HasAny()) return affected;

            FilterDefinition<T> idsFilter = Builders<T>.Filter.In((ids is IEnumerable<Guid>)
                ?
                ReservedColumnName.GlobalUId
                :
                ReservedColumnName._Id.ToLower(), ids);
            FilterDefinition<T> filter = Builders<T>.Filter.And(idsFilter, GenerateEntityStatusFilter<T>(RecordMode.SoftDeleted));
            UpdateDefinition<T> updateDefinition = Builders<T>.Update
                .Set(x => x.EntityStatus, EntityStatus.Active)
                .Set(y => y.ModifiedOn, SystemDate.UtcNow);
            IList<WriteModel<T>> recordsToRestore = [new UpdateManyModel<T>(filter, updateDefinition)];
            IMongoCollection<T> documentCollection = GetCollection<T>();
            BulkWriteResult<T> result = await documentCollection.BulkWriteAsync(recordsToRestore, null, cancellationToken).ConfigureAwait(false);
            bool bulkRestored = IsValidBulkWriteResult(result) && result.ModifiedCount > 0;

            if (bulkRestored)
            {
                affected = (int)result.ModifiedCount;
            }

            return affected;
        }

        private async Task<T> FindRecord<T>(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
            where T : class, IReadModel<string>
        {
            IMongoCollection<T> documentCollection = GetCollection<T>();

            return await documentCollection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<PagedResult<T>> GetRecords<T>(
            int skip = 0,
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

            predicate ??= x => true;

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
                record.CreatedAt = SystemDate.UtcNow;
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
