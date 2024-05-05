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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModels;

namespace CrystalSharp.Infrastructure.ReadModelStoresPersistence
{
    public interface IReadModelStore<TKey>
    {
        Task<bool> Store<T>(T record, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> BulkStore<T>(IEnumerable<T> records, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> Update<T>(T record, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> Delete<T>(TKey id, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> Delete<T>(Guid globalUId, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> SoftDelete<T>(TKey id, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> SoftDelete<T>(Guid globalUId, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> BulkDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> BulkDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> BulkSoftDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> BulkSoftDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> Restore<T>(TKey id, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> Restore<T>(Guid globalUId, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> BulkRestore<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<bool> BulkRestore<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<long> Count<T>(RecordMode recordMode = RecordMode.Active, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<T> Find<T>(TKey id, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<T> Find<T>(Guid globalUId, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<PagedResult<T>> Get<T>(int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>;
        Task<PagedResult<T>> Search<T>(string term,
            bool useWildcard,
            int skip = 0,
            int take = 10,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>;
    }
}
