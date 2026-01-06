using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.ReadModels;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CrystalSharp.MongoDb.Extensions
{
    public static class MongoDbQueryableExtensions
    {
        public static IQueryable<T> Get<T, TKey>(
            this IMongoCollection<T> dbCollection,
            int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None)
            where T : IReadModel<TKey>
        {
            IQueryable<T> records;
            Expression<Func<T, bool>> entityStatusPredicate;

            if (recordMode == RecordMode.All)
            {
                entityStatusPredicate = x => x.EntityStatus == EntityStatus.Active || x.EntityStatus == EntityStatus.Deleted;
            }
            else
            {
                if (recordMode == RecordMode.Active)
                {
                    entityStatusPredicate = x => x.EntityStatus == EntityStatus.Active;
                }
                else
                {
                    entityStatusPredicate = x => x.EntityStatus == EntityStatus.Deleted;
                }
            }

            predicate ??= x => true;

            if (sortMode == DataSortMode.None)
            {
                records = dbCollection.AsQueryable<T>().Where(entityStatusPredicate).Where(predicate).Skip(skip).Take(take);
            }
            else
            {
                records = (sortMode == DataSortMode.Ascending)
                    ?
                    dbCollection.AsQueryable<T>().Where(entityStatusPredicate).Where(predicate).OrderBy(sortColumn).Skip(skip).Take(take)
                    :
                    dbCollection.AsQueryable<T>().Where(entityStatusPredicate).Where(predicate).OrderByDescending(sortColumn).Skip(skip).Take(take);
            }

            return records;
        }
    }
}
