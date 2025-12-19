using CrystalSharp.Application;
using CrystalSharp.Application.Handlers;
using CrystalSharp.Common.Settings;
using CrystalSharp.Envoy;
using CrystalSharp.Envoy.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public abstract class SagaTransactionHandler<TRequest> : Handler, IRequestHandler<TRequest, SagaTransactionResult>
        where TRequest : IRequest<SagaTransactionResult>
    {
        public abstract Task<SagaTransactionResult> Handle(TRequest request, CancellationToken cancellationToken = default);

        protected Task<SagaTransactionResult> Ok(Guid correlationId)
        {
            SagaTransactionResult result = new(correlationId, true);

            return Task.FromResult(result);
        }

        protected Task<SagaTransactionResult> Fail(Guid correlationId, params string[] errorMessages)
        {
            IEnumerable<Error> errors = errorMessages.ToList().Select(x => new Error(ReservedErrorCode.SystemError, x));
            SagaTransactionResult result = new(correlationId, false, errors);

            return Task.FromResult(result);
        }
    }
}
