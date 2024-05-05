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

using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Sagas
{
    public abstract class SagaTransactionAssistant<TRequest> : SagaTransactionHandler<TRequest>
        where TRequest : IRequest<SagaTransactionResult>
    {
        protected async Task<SagaTransactionMeta> GetSagaTransaction(ISagaStore sagaStore,
            string sagaId,
            string startedBy,
            string step,
            CancellationToken cancellationToken = default)
        {
            SagaTransactionMeta sagaTransactionMeta = await ExistingSagaTransaction(sagaStore, sagaId, cancellationToken).ConfigureAwait(false);

            if (sagaTransactionMeta == null)
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

            if (sagaTransactionMeta != null)
            {
                sagaTransactionMeta.State = success ? SagaState.Committed : SagaState.Aborted;

                if (!string.IsNullOrEmpty(errorTrail))
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

        protected async Task<SagaTransactionMeta> ExistingSagaTransaction(ISagaStore sagaStore,
            string sagaId,
            CancellationToken cancellationToken = default)
        {
            SagaTransactionMeta sagaTransactionMeta = await sagaStore.Get(sagaId, cancellationToken).ConfigureAwait(false);

            return sagaTransactionMeta;
        }
    }
}
