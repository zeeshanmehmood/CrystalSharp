using CrystalSharp.Common.Extensions;
using CrystalSharp.Envoy.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public abstract class SagaTransactionAssistant<TRequest> : SagaTransactionHandler<TRequest>
        where TRequest : IRequest<SagaTransactionResult>
    {
        protected async Task<SagaTransactionMeta> GetSagaTransaction(
            ISagaStore sagaStore,
            string sagaId,
            string startedBy,
            string step,
            CancellationToken cancellationToken = default)
        {
            SagaTransactionMeta sagaTransactionMeta = await ExistingSagaTransaction(sagaStore, sagaId, cancellationToken).ConfigureAwait(false);

            if (sagaTransactionMeta is null)
            {
                sagaTransactionMeta = NewSagaTransaction(sagaId, startedBy, step);
            }
            else
            {
                sagaTransactionMeta.Step = step;
                sagaTransactionMeta.State = SagaState.Active;
            }

            return sagaTransactionMeta;
        }

        protected async Task Windup(ISagaStore sagaStore, string sagaId, bool success, string errorTrail, CancellationToken cancellationToken = default)
        {
            SagaTransactionMeta sagaTransactionMeta = await sagaStore.Get(sagaId, cancellationToken).ConfigureAwait(false);

            if (sagaTransactionMeta is not null)
            {
                sagaTransactionMeta.State = success ? SagaState.Committed : SagaState.Aborted;

                if (errorTrail.IsValidString())
                {
                    sagaTransactionMeta.ErrorTrail = errorTrail;
                }

                await sagaStore.Upsert(sagaTransactionMeta, cancellationToken).ConfigureAwait(false);
            }
        }

        protected SagaTransactionMeta NewSagaTransaction(string sagaId, string startedBy, string step)
        {
            return new SagaTransactionMeta { CorrelationId = sagaId, StartedBy = startedBy, Step = step, State = SagaState.New };
        }

        protected async Task<SagaTransactionMeta> ExistingSagaTransaction(
            ISagaStore sagaStore,
            string sagaId,
            CancellationToken cancellationToken = default)
        {
            SagaTransactionMeta sagaTransactionMeta = await sagaStore.Get(sagaId, cancellationToken).ConfigureAwait(false);

            return sagaTransactionMeta;
        }
    }
}
