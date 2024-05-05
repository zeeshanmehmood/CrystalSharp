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
using CrystalSharp.Sagas.Exceptions;

namespace CrystalSharp.Sagas
{
    public abstract class SagaOrchestrator<TSagaLocator, TRequest> : SagaTransactionAssistant<TRequest>, ISagaOrchestrator<TSagaLocator, TRequest>
        where TSagaLocator : ISagaLocator
        where TRequest : IRequest<SagaTransactionResult>
    {
        private readonly IResolver _resolver;
        private readonly ISagaStore _sagaStore;
        private readonly TSagaLocator _sagaLocator;
        private TRequest _initialTransaction;
        private IList<SagaActivityStore> _activities;

        protected SagaOrchestrator(IResolver resolver, ISagaStore sagaStore, TSagaLocator sagaLocator)
        {
            _resolver = resolver;
            _sagaStore = sagaStore;
            _sagaLocator = sagaLocator;
        }

        public ISagaOrchestrator<TSagaLocator, TRequest> PrepareOrchestrator(TRequest initialTransaction)
        {
            _initialTransaction = initialTransaction;
            _activities = new List<SagaActivityStore>();

            return this;
        }

        public ISagaOrchestrator<TSagaLocator, TRequest> Activity<TActivity>(string name) where TActivity : ISagaActivity
        {
            if (_activities != null)
            {
                SagaActivityStore existingSagaActiveStore = _activities.Where(x => x.ActivityName.ToLower() == name.ToLower()).SingleOrDefault();

                if (existingSagaActiveStore != null)
                {
                    string errorMessage = $"Duplicate activity name. The activity \"{name}\" already exists.";

                    throw new SagaDuplicateActivityNameException(ReservedErrorCode.SystemError, errorMessage);
                }

                _activities.Add(new SagaActivityStore { ActivityName = name, ActivityType = typeof(TActivity) });
            }

            return this;
        }

        public ISagaOrchestrator<TSagaLocator, TRequest> WithCompensation<TCompensation>(string name) where TCompensation : ISagaActivity
        {
            SagaActivityStore existingActivity = _activities.LastOrDefault();

            if (existingActivity == null)
            {
                string errorMessage = "Cannot set compensation. There are no activities defined.";

                throw new SagaZeroActivitiesException(ReservedErrorCode.SystemError, errorMessage);
            }

            existingActivity.CompensationName = name;
            existingActivity.CompensationType = typeof(TCompensation);

            return this;
        }

        public async Task<SagaResult> Run(CancellationToken cancellationToken = default)
        {
            Guid correlationId = Guid.NewGuid();

            return await Run(correlationId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SagaResult> Run(Guid correlationId, CancellationToken cancellationToken = default)
        {
            if (!_activities.HasAny())
            {
                string errorMessage = "The saga cannot run. There are no activities defined.";

                throw new SagaZeroActivitiesException(ReservedErrorCode.SystemError, errorMessage);
            }

            bool hasError = false;
            string sagaId = await _sagaLocator.Locate(correlationId);
            IList<ISagaActivity> compensations = new List<ISagaActivity>();
            SagaOrchestratorContext context = new(correlationId, _initialTransaction);
            bool success = false;
            IList<SagaTrail> trail = new List<SagaTrail>();

            foreach (SagaActivityStore activity in _activities)
            {
                ISagaActivity currentActivity = _resolver.CreateInstance<ISagaActivity>(activity.ActivityType);
                ISagaActivity compensation = null;

                if (activity.CompensationType != null)
                {
                    compensation = _resolver.CreateInstance<ISagaActivity>(activity.CompensationType);

                    if (compensation != null)
                    {
                        compensations.Add(compensation);
                    }
                }

                SagaTransactionMeta sagaTransactionMeta = await GetSagaTransaction(_sagaStore,
                    sagaId,
                    typeof(TRequest).Name,
                    activity.ActivityName,
                    cancellationToken)
                    .ConfigureAwait(false);
                SagaTrail trailItem = await ProcessActivity(sagaTransactionMeta,
                    context,
                    currentActivity,
                    cancellationToken)
                    .ConfigureAwait(false);

                trail.Add(trailItem);

                if (!trailItem.Success)
                {
                    hasError = true;

                    break;
                }
            }

            if (hasError)
            {
                if (compensations.Any())
                {
                    _ = compensations.Reverse();

                    foreach (ISagaActivity compensationActivity in compensations)
                    {
                        await compensationActivity.Execute(context, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                success = true;
            }

            IEnumerable<Error> errors = trail.Where(t => t.Errors.HasAny()).SelectMany(e => e.Errors);
            string errorTrail = errors.HasAny() ? Serializer.Serialize(errors) : null;

            await Windup(_sagaStore, sagaId, success, errorTrail, cancellationToken).ConfigureAwait(false);

            SagaResult sagaResult = new(correlationId, success, trail);

            return sagaResult;
        }

        private async Task<SagaTrail> ProcessActivity(SagaTransactionMeta sagaTransactionMeta,
            SagaOrchestratorContext context,
            ISagaActivity activity,
            CancellationToken cancellationToken = default)
        {
            bool success = false;
            IEnumerable<Error> errors = null;

            try
            {
                if (sagaTransactionMeta.State == SagaState.New || sagaTransactionMeta.State == SagaState.Active)
                {
                    await _sagaStore.Upsert(sagaTransactionMeta, cancellationToken).ConfigureAwait(false);

                    SagaTransactionResult sagaTransactionResult = await activity.Execute(context, cancellationToken).ConfigureAwait(false);
                    success = sagaTransactionResult != null && sagaTransactionResult.Success;

                    if (!success)
                    {
                        if (sagaTransactionResult != null && sagaTransactionResult.Errors.Any())
                        {
                            errors = sagaTransactionResult.Errors;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                errors = new List<Error> { new Error(ReservedErrorCode.SystemError, exception.Message) };
            }

            return new SagaTrail(sagaTransactionMeta.Step, success, errors);
        }
    }
}
