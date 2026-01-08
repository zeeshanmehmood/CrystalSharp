using CrystalSharp.Application;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Envoy.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public abstract class SagaChoreography<TSagaLocator, TRequest>(
        ISagaStore sagaStore,
        TSagaLocator sagaLocator,
        ISagaTransactionExecutor sagaTransactionExecutor) : SagaTransactionAssistant<TRequest>
        where TRequest : IRequest<SagaTransactionResult>
        where TSagaLocator : ISagaLocator
    {
        private readonly ISagaStore _sagaStore = sagaStore;
        private readonly TSagaLocator _sagaLocator = sagaLocator;
        private readonly ISagaTransactionExecutor _sagaTransactionExecutor = sagaTransactionExecutor;

        protected async Task<SagaResult> Execute(
            ISagaTransaction transaction,
            Func<Task> compensation,
            CancellationToken cancellationToken = default)
        {
            Guid correlationId = Guid.Create();

            return await Execute(correlationId, transaction, compensation, cancellationToken).ConfigureAwait(false);
        }

        protected async Task<SagaResult> Execute(
            Guid correlationId,
            ISagaTransaction transaction,
            Func<Task> compensation,
            CancellationToken cancellationToken = default)
        {
            string sagaId = await _sagaLocator.Locate(correlationId);
            SagaTransactionMeta sagaTransactionMeta = await GetSagaTransaction(
                _sagaStore,
                sagaId,
                typeof(TRequest).Name,
                transaction.GetType().Name,
                cancellationToken)
                .ConfigureAwait(false);
            SagaTrail trailItem = await ExecuteTransaction(
                correlationId,
                sagaTransactionMeta,
                transaction,
                compensation,
                cancellationToken)
                .ConfigureAwait(false);
            IEnumerable<SagaTrail> trail = [trailItem];

            if (!trailItem.Success)
            {
                IEnumerable<Error> errors = trail.Where(t => t.Errors.HasAny()).SelectMany(e => e.Errors);
                string errorTrail = errors.HasAny() ? Serializer.Serialize(errors) : null;

                await MarkAsFail(correlationId, errorTrail, cancellationToken).ConfigureAwait(false);
            }

            SagaResult sagaResult = new(correlationId, trailItem.Success, trail);

            return sagaResult;
        }

        protected async Task MarkAsComplete(Guid correlationId, CancellationToken cancellationToken = default)
        {
            await SetFinalState(correlationId, true, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task MarkAsFail(Guid correlationId, string errorTrail, CancellationToken cancellationToken = default)
        {
            await SetFinalState(correlationId, false, errorTrail, cancellationToken).ConfigureAwait(false);
        }

        private async Task SetFinalState(Guid correlationId, bool success, string errorTrail, CancellationToken cancellationToken = default)
        {
            string sagaId = await _sagaLocator.Locate(correlationId);

            await Windup(_sagaStore, sagaId, success, errorTrail, cancellationToken).ConfigureAwait(false);
        }

        private async Task<SagaTrail> ExecuteTransaction(
            Guid correlationId,
            SagaTransactionMeta sagaTransactionMeta,
            ISagaTransaction transaction,
            Func<Task> compensation,
            CancellationToken cancellationToken = default)
        {
            bool hasError = false;
            bool success = false;
            IEnumerable<Error> errors = null;

            try
            {
                if (sagaTransactionMeta.State == SagaState.New || sagaTransactionMeta.State == SagaState.Active)
                {
                    await _sagaStore.Upsert(sagaTransactionMeta, cancellationToken).ConfigureAwait(false);

                    SagaTransactionResult sagaTransactionResult = await _sagaTransactionExecutor.Execute(transaction, cancellationToken);
                    success = sagaTransactionResult is not null && sagaTransactionResult.Success;

                    if (!success)
                    {
                        hasError = true;

                        if (sagaTransactionResult is not null && sagaTransactionResult.Errors.HasAny())
                        {
                            errors = sagaTransactionResult.Errors;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                hasError = true;
                errors = [new Error(ReservedErrorCode.SystemError, exception.Message)];
            }

            if (hasError)
            {
                compensation?.Invoke();
            }

            return new SagaTrail(sagaTransactionMeta.Step, success, errors);
        }
    }
}
