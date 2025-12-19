using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Execution
{
    public interface IQueryExecutor
    {
        Task<QueryExecutionResult<TResult>> Execute<TResult>(IQuery<QueryExecutionResult<TResult>> query, CancellationToken cancellationToken = default);
    }
}
