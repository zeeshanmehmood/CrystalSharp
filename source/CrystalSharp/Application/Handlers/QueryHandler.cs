using CrystalSharp.Common.Settings;
using CrystalSharp.Envoy;
using CrystalSharp.Envoy.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Handlers
{
    public abstract class QueryHandler<TRequest, TResponse> : Handler, IRequestHandler<TRequest, QueryExecutionResult<TResponse>>
        where TRequest : IRequest<QueryExecutionResult<TResponse>>
    {
        public abstract Task<QueryExecutionResult<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default);

        protected Task<QueryExecutionResult<TResponse>> Ok(TResponse data)
        {
            QueryExecutionResult<TResponse> result = new() { Success = true, Data = data };

            return Task.FromResult(result);
        }

        protected Task<QueryExecutionResult<IEnumerable<TResponse>>> Ok(IEnumerable<TResponse> data)
        {
            QueryExecutionResult<IEnumerable<TResponse>> result = new() { Success = true, Data = data };

            return Task.FromResult(result);
        }

        protected Task<QueryExecutionResult<TResponse>> Fail(params string[] errorMessages)
        {
            QueryExecutionResult<TResponse> result = new()
            {
                Success = false,
                Errors = errorMessages.ToList().Select(x => new Error(ReservedErrorCode.SystemError, x))
            };

            return Task.FromResult(result);
        }
    }
}
