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
using Microsoft.EntityFrameworkCore;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.EntityFrameworkCore.Common.Extensions;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModels;

namespace CrystalSharp.EntityFrameworkCore.Common.Stores
{
    public abstract class ReadModelStore<TDbContext, TKey> where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        protected ReadModelStore(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<bool> Store<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            await _dbContext.Set<T>().AddAsync(record, cancellationToken).ConfigureAwait(false);

            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        public virtual async Task<bool> BulkStore<T>(IEnumerable<T> records, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!records.HasAny()) return false;

            await _dbContext.Set<T>().AddRangeAsync(records, cancellationToken).ConfigureAwait(false);

            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        public virtual async Task<bool> Update<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            _dbContext.Set<T>().Update(record);

            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        public virtual async Task<bool> Delete<T>(TKey id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            bool deleted = await DeleteRecord<T, TKey>(id, cancellationToken).ConfigureAwait(false);

            return deleted;
        }

        public virtual async Task<bool> Delete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            bool deleted = await DeleteRecord<T, Guid>(globalUId, cancellationToken).ConfigureAwait(false);

            return deleted;
        }

        public virtual async Task<bool> SoftDelete<T>(TKey id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            bool deleted = await SoftDeleteRecord<T, TKey>(id, cancellationToken).ConfigureAwait(false);

            return deleted;
        }

        public virtual async Task<bool> SoftDelete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            bool deleted = await SoftDeleteRecord<T, Guid>(globalUId, cancellationToken).ConfigureAwait(false);

            return deleted;
        }

        public virtual async Task<bool> BulkDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!ids.HasAny()) return false;

            bool bulkDeleted = await BulkDeleteRecords<T, TKey>(ids, cancellationToken).ConfigureAwait(false);

            return bulkDeleted;
        }

        public virtual async Task<bool> BulkDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!globalUIds.HasAny()) return false;

            bool bulkDeleted = await BulkDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);

            return bulkDeleted;
        }

        public virtual async Task<bool> BulkSoftDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!ids.HasAny()) return false;

            bool bulkDeleted = await BulkSoftDeleteRecords<T, TKey>(ids, cancellationToken).ConfigureAwait(false);

            return bulkDeleted;
        }

        public virtual async Task<bool> BulkSoftDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!globalUIds.HasAny()) return false;

            bool bulkDeleted = await BulkSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);

            return bulkDeleted;
        }

        public virtual async Task<bool> Restore<T>(TKey id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            bool restored = await RestoreSoftDeletedRecord<T, TKey>(id, cancellationToken).ConfigureAwait(false);

            return restored;
        }

        public virtual async Task<bool> Restore<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            bool restored = await RestoreSoftDeletedRecord<T, Guid>(globalUId, cancellationToken).ConfigureAwait(false);

            return restored;
        }

        public virtual async Task<bool> BulkRestore<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!ids.HasAny()) return false;

            bool restored = await BulkRestoreSoftDeleteRecords<T, TKey>(ids, cancellationToken).ConfigureAwait(false);

            return restored;
        }

        public virtual async Task<bool> BulkRestore<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!globalUIds.HasAny()) return false;

            bool restored = await BulkRestoreSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);

            return restored;
        }

        public virtual async Task<long> Count<T>(RecordMode recordMode = RecordMode.Active, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = GenerateEntityStatusPredicate<T>(recordMode);
            long totalCount = await _dbContext.Set<T>().Where(predicate).LongCountAsync(cancellationToken).ConfigureAwait(false);

            return totalCount;
        }

        public virtual async Task<long> Count<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            long totalCount = await _dbContext.Set<T>().Where(predicate).LongCountAsync(cancellationToken).ConfigureAwait(false);

            return totalCount;
        }

        public virtual async Task<T> Find<T>(TKey id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = GenerateIdAndEntityStatusPredicate<T, TKey>(id);
            T existing = await _dbContext.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

            return existing;
        }

        public virtual async Task<T> Find<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = GenerateIdAndEntityStatusPredicate<T, Guid>(globalUId);
            T existing = await _dbContext.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

            return existing;
        }

        public virtual async Task<PagedResult<T>> Get<T>(int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            PagedResult<T> result = await GetRecords(skip, take, predicate, recordMode, sortColumn, sortMode, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public virtual async Task<PagedResult<T>> Search<T>(string term,
            bool useWildcard,
            int skip = 0,
            int take = 10,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            await Task.CompletedTask;

            throw new NotImplementedException("NOTE: Use \"Get\" method for search.");
        }

        private Expression<Func<T, bool>> GenerateIdPredicate<T, TId>(TId id)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = x => (id is Guid) ? x.GlobalUId.Equals(id) : x.Id.Equals(id);

            return predicate;
        }

        private Expression<Func<T, bool>> GenerateEntityStatusPredicate<T>(RecordMode recordMode = RecordMode.Active)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate;

            if (recordMode == RecordMode.Active || recordMode == RecordMode.SoftDeleted)
            {
                predicate = x => (recordMode == RecordMode.Active) ? x.EntityStatus == EntityStatus.Active : x.EntityStatus == EntityStatus.Deleted;
            }
            else
            {
                predicate = x => true;
            }

            return predicate;
        }

        private Expression<Func<T, bool>> GenerateIdAndEntityStatusPredicate<T, TId>(TId id, EntityStatus entityStatus = EntityStatus.Active)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = x => (id is Guid) ? x.GlobalUId.Equals(id) && x.EntityStatus == entityStatus : x.Id.Equals(id) && x.EntityStatus == entityStatus;

            return predicate;
        }

        private async Task<bool> DeleteRecord<T, TId>(TId id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = GenerateIdPredicate<T, TId>(id);
            T existing = await _dbContext.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

            if (existing == null) return false;

            _dbContext.Set<T>().Remove(existing);

            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        private async Task<bool> SoftDeleteRecord<T, TId>(TId id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = GenerateIdAndEntityStatusPredicate<T, TId>(id);
            T existing = await _dbContext.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

            if (existing == null) return false;

            existing.EntityStatus = EntityStatus.Deleted;
            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        private async Task<bool> BulkDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate;

            if (ids is IEnumerable<Guid>)
            {
                IEnumerable<Guid> listUIds = ids.Select(x => Guid.Parse(x.ToString()));
                predicate = x => listUIds.Contains(x.GlobalUId);
            }
            else
            {
                IEnumerable<TKey> listIds = ids.Select(x => (TKey)Convert.ChangeType(x, typeof(TKey)));
                predicate = x => listIds.Contains(x.Id);
            }

            IQueryable<T> existingRecords = _dbContext.Set<T>().Where(predicate);

            if (!existingRecords.HasAny()) return false;

            _dbContext.RemoveRange(existingRecords);

            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        private async Task<bool> BulkSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate;

            if (ids is IEnumerable<Guid>)
            {
                IEnumerable<Guid> listUIds = ids.Select(x => Guid.Parse(x.ToString()));
                predicate = x => listUIds.Contains(x.GlobalUId) && x.EntityStatus == EntityStatus.Active;
            }
            else
            {
                IEnumerable<TKey> listIds = ids.Select(x => (TKey)Convert.ChangeType(x, typeof(TKey)));
                predicate = x => listIds.Contains(x.Id) && x.EntityStatus == EntityStatus.Active;
            }

            IQueryable<T> existingRecords = _dbContext.Set<T>().Where(predicate);

            if (!existingRecords.HasAny()) return false;

            foreach (T record in existingRecords)
            {
                record.EntityStatus = EntityStatus.Deleted;
            }

            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        private async Task<bool> RestoreSoftDeletedRecord<T, TId>(TId id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate = GenerateIdAndEntityStatusPredicate<T, TId>(id, EntityStatus.Deleted);
            T existingRecord = await _dbContext.Set<T>().Where(predicate).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (existingRecord == null) return false;

            existingRecord.EntityStatus = EntityStatus.Active;
            bool restored = await Update(existingRecord, cancellationToken).ConfigureAwait(false);

            return restored;
        }

        private async Task<bool> BulkRestoreSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            Expression<Func<T, bool>> predicate;

            if (ids is IEnumerable<Guid>)
            {
                IEnumerable<Guid> listUIds = ids.Select(x => Guid.Parse(x.ToString()));
                predicate = x => listUIds.Contains(x.GlobalUId) && x.EntityStatus == EntityStatus.Deleted;
            }
            else
            {
                IEnumerable<TKey> listIds = ids.Select(x => (TKey)Convert.ChangeType(x, typeof(TKey)));
                predicate = x => listIds.Contains(x.Id) && x.EntityStatus == EntityStatus.Deleted;
            }

            IQueryable<T> existingRecords = _dbContext.Set<T>().Where(predicate);

            if (!existingRecords.HasAny()) return false;

            foreach (T record in existingRecords)
            {
                record.EntityStatus = EntityStatus.Active;
            }

            int affected = await SaveChanges(_dbContext, cancellationToken).ConfigureAwait(false);

            return affected > 0;
        }

        private async Task<PagedResult<T>> GetRecords<T>(int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            await Task.CompletedTask;

            long totalRecords = 0;
            Expression<Func<T, bool>> recordModePredicate;

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

            totalRecords = _dbContext.Set<T>().Where(predicate).Where(recordModePredicate).LongCount();
            IQueryable<T> records = _dbContext.Get<T, TKey>(skip, take, predicate, recordMode, sortColumn, sortMode);
            PagedResult<T> result = null;

            if (records.HasAny())
            {
                result = new PagedResult<T>(skip, take, totalRecords, records);
            }

            return result;
        }

        private void DateTraction(TDbContext dbContext)
        {
            var modifiedItems = dbContext.ChangeTracker
                .Entries<IReadModel<TKey>>()
                .Where(entity => entity.State == EntityState.Modified);

            var newItems = dbContext.ChangeTracker
                .Entries<IReadModel<TKey>>()
                .Where(entity => entity.State == EntityState.Added);

            foreach (var item in modifiedItems)
            {
                item.Entity.ModifiedOn = SystemDate.UtcNow;
            }

            foreach (var item in newItems)
            {
                item.Entity.CreatedOn = SystemDate.UtcNow;
            }
        }

        private async Task<int> SaveChanges(TDbContext dbContext, CancellationToken cancellationToken = default)
        {
            DateTraction(dbContext);

            int affected = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return affected;
        }
    }
}
