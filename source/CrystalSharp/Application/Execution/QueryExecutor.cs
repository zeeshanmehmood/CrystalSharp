using CrystalSharp.Common.Settings;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Envoy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Execution
{
    public class QueryExecutor(IEnvoy envoy) : IQueryExecutor
    {
        private readonly IEnvoy _envoy = envoy;

        public async Task<QueryExecutionResult<TResult>> Execute<TResult>(IQuery<QueryExecutionResult<TResult>> query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _envoy.Send(query, cancellationToken).ConfigureAwait(false);
            }
            catch (DomainException exception)
            {
                return QueryExecutionResult<TResult>.WithError([new(exception.ErrorCode, exception.Message)]);
            }
            catch (Exception exception)
            {
                return QueryExecutionResult<TResult>.WithError([new(ReservedErrorCode.SystemError, exception.Message)]);
            }
        }
    }
}
