using CrystalSharp.Common.Settings;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Envoy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public class SagaTransactionExecutor(IEnvoy envoy) : ISagaTransactionExecutor
    {
        private readonly IEnvoy _envoy = envoy;

        public async Task<SagaTransactionResult> Execute(ISagaTransaction transaction, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _envoy.Send(transaction, cancellationToken).ConfigureAwait(false);
            }
            catch (DomainException exception)
            {
                return SagaTransactionResult.WithError([new(exception.ErrorCode, exception.Message)]);
            }
            catch (Exception exception)
            {
                return SagaTransactionResult.WithError([new(ReservedErrorCode.SystemError, exception.Message)]);
            }
        }
    }
}
