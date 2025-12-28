using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.ReadModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CrystalSharp.EntityFrameworkCore.Common.Extensions
{
    public static class EntityFrameworkCoreDbContextExtensions
    {
        public static IQueryable<T> Get<T, TKey>(
            this DbContext dbContext,
            int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            bool tracking = false,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None)
            where T : class, IReadModel<TKey>
        {
            IQueryable<T> records;
            Expression<Func<T, bool>> entityStatusPredicate;

            if (recordMode == RecordMode.All)
            {
                entityStatusPredicate = x => x.EntityStatus == EntityStatus.Active || x.EntityStatus == EntityStatus.Deleted;
            }
            else
            {
                entityStatusPredicate = x => (recordMode == RecordMode.Active) ? x.EntityStatus == EntityStatus.Active : x.EntityStatus == EntityStatus.Deleted;
            }

            predicate ??= x => true;

            if (sortMode == DataSortMode.None)
            {
                records = tracking
                    ?
                    dbContext.Set<T>().Where(entityStatusPredicate).Where(predicate).Skip(skip).Take(take)
                    :
                    dbContext.Set<T>().AsNoTracking().Where(entityStatusPredicate).Where(predicate).Skip(skip).Take(take);
            }
            else
            {
                records = (sortMode == DataSortMode.Ascending)
                    ?
                    GetAscending<T, TKey>(dbContext, tracking, entityStatusPredicate, predicate, sortColumn, skip, take)
                    :
                    GetDescending<T, TKey>(dbContext, tracking, entityStatusPredicate, predicate, sortColumn, skip, take);
            }

            return records;
        }

        private static IQueryable<T> GetAscending<T, TKey>(
            DbContext dbContext,
            bool tracking,
            Expression<Func<T, bool>> entityStatusPredicate,
            Expression<Func<T, bool>> predicate,
            string sortColumn,
            int skip,
            int take)
            where T : class, IReadModel<TKey>
        {
            IQueryable<T> records = tracking
                ?
                dbContext.Set<T>().Where(entityStatusPredicate).Where(predicate).OrderBy(sortColumn).Skip(skip).Take(take)
                :
                dbContext.Set<T>().AsNoTracking().Where(entityStatusPredicate).Where(predicate).OrderBy(sortColumn).Skip(skip).Take(take);

            return records;
        }

        private static IQueryable<T> GetDescending<T, TKey>(
            DbContext dbContext,
            bool tracking,
            Expression<Func<T, bool>> entityStatusPredicate,
            Expression<Func<T, bool>> predicate,
            string sortColumn,
            int skip,
            int take)
            where T : class, IReadModel<TKey>
        {
            IQueryable<T> records = tracking
                ?
                dbContext.Set<T>().Where(entityStatusPredicate).Where(predicate).OrderByDescending(sortColumn).Skip(skip).Take(take)
                :
                dbContext.Set<T>().AsNoTracking().Where(entityStatusPredicate).Where(predicate).OrderByDescending(sortColumn).Skip(skip).Take(take);

            return records;
        }
    }
}
