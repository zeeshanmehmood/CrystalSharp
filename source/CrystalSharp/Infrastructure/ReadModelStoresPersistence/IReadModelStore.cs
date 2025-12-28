using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure.ReadModelStoresPersistence
{
    public interface IReadModelStore<TKey>
    {
        Task<int> Store<T>(T record, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> BulkStore<T>(IEnumerable<T> records, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> Update<T>(T record, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> Delete<T>(TKey id, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> Delete<T>(Guid globalUId, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> SoftDelete<T>(TKey id, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> SoftDelete<T>(Guid globalUId, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> BulkDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> BulkDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> BulkSoftDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> BulkSoftDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> Restore<T>(TKey id, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> Restore<T>(Guid globalUId, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> BulkRestore<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<int> BulkRestore<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<long> Count<T>(RecordMode recordMode = RecordMode.Active, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<long> Count<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<T> Find<T>(TKey id, bool tracking = false, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<T> Find<T>(Guid globalUId, bool tracking = false, CancellationToken cancellationToken = default) where T : class, IReadModel<TKey>;
        Task<IQueryable<T>> Filter<T>(Expression<Func<T, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>;
        Task<PagedResult<T>> Get<T>(int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            bool tracking = false,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>;
    }
}
