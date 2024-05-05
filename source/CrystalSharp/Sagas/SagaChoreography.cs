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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Application;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Sagas
{
    public abstract class SagaChoreography<TSagaLocator, TRequest> : SagaTransactionAssistant<TRequest> 
        where TRequest : IRequest<SagaTransactionResult>
        where TSagaLocator : ISagaLocator
    {
        private readonly ISagaStore _sagaStore;
        private readonly TSagaLocator _sagaLocator;
        private readonly ISagaTransactionExecutor _sagaTransactionExecutor;

        protected SagaChoreography(ISagaStore sagaStore, TSagaLocator sagaLocator, ISagaTransactionExecutor sagaTransactionExecutor)
        {
            _sagaStore = sagaStore;
            _sagaLocator = sagaLocator;
            _sagaTransactionExecutor = sagaTransactionExecutor;
        }

        protected async Task<SagaResult> Execute(ISagaTransaction transaction,
            Func<Task> compensation,
            CancellationToken cancellationToken = default)
        {
            Guid correlationId = Guid.NewGuid();

            return await Execute(correlationId, transaction, compensation, cancellationToken).ConfigureAwait(false);
        }

        protected async Task<SagaResult> Execute(Guid correlationId,
            ISagaTransaction transaction,
            Func<Task> compensation,
            CancellationToken cancellationToken = default)
        {
            string sagaId = await _sagaLocator.Locate(correlationId);
            SagaTransactionMeta sagaTransactionMeta = await GetSagaTransaction(_sagaStore,
                sagaId,
                typeof(TRequest).Name,
                transaction.GetType().Name,
                cancellationToken)
                .ConfigureAwait(false);
            SagaTrail trailItem = await ExecuteTransaction(correlationId,
                sagaTransactionMeta,
                transaction,
                compensation,
                cancellationToken)
                .ConfigureAwait(false);
            IEnumerable<SagaTrail> trail = new List<SagaTrail> { trailItem };

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

        private async Task<SagaTrail> ExecuteTransaction(Guid correlationId,
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
                    success = sagaTransactionResult != null && sagaTransactionResult.Success;

                    if (!success)
                    {
                        hasError = true;

                        if (sagaTransactionResult != null && sagaTransactionResult.Errors.HasAny())
                        {
                            errors = sagaTransactionResult.Errors;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                hasError = true;
                errors = new List<Error> { new Error(ReservedErrorCode.SystemError, exception.Message) };
            }

            if (hasError)
            {
                compensation?.Invoke();
            }

            return new SagaTrail(sagaTransactionMeta.Step, success, errors);
        }
    }
}
