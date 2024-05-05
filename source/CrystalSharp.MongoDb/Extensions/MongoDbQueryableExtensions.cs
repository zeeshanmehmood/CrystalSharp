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
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.ReadModels;

namespace CrystalSharp.MongoDb.Extensions
{
    public static class MongoDbQueryableExtensions
    {
        public static IQueryable<T> Get<T, TKey>(this IMongoCollection<T> dbCollection,
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

            if (predicate == null)
            {
                predicate = x => true;
            }

            if (sortMode == DataSortMode.None)
            {
                records = dbCollection.AsQueryable<T>().Where(entityStatusPredicate).Where(predicate).Skip(skip).Take(take);
            }
            else
            {
                records = (sortMode == DataSortMode.Ascending)
                    ?
                    dbCollection.AsQueryable<T>().Where(entityStatusPredicate).Where(predicate).Skip(skip).Take(take).OrderBy(sortColumn)
                    :
                    dbCollection.AsQueryable<T>().Where(entityStatusPredicate).Where(predicate).Skip(skip).Take(take).OrderByDescending(sortColumn);
            }

            return records;
        }
    }
}
