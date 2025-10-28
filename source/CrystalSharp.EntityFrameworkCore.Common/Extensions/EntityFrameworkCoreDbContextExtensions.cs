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
using Microsoft.EntityFrameworkCore;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.ReadModels;

namespace CrystalSharp.EntityFrameworkCore.Common.Extensions
{
    public static class EntityFrameworkCoreDbContextExtensions
    {
        public static IQueryable<T> Get<T, TKey>(this DbContext dbContext,
            int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
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

            if (predicate == null)
            {
                predicate = x => true;
            }

            if (sortMode == DataSortMode.None)
            {
                records = dbContext.Set<T>().Where(entityStatusPredicate).Where(predicate).Skip(skip).Take(take);
            }
            else
            {
                records = (sortMode == DataSortMode.Ascending) 
                    ?
                    dbContext.Set<T>().Where(entityStatusPredicate).Where(predicate).OrderBy(sortColumn).Skip(skip).Take(take)
                    :
                    dbContext.Set<T>().Where(entityStatusPredicate).Where(predicate).OrderByDescending(sortColumn).Skip(skip).Take(take);
            }

            return records;
        }
    }
}
