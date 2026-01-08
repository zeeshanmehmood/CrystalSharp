using CrystalSharp.Application;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Envoy.Contracts;
using CrystalSharp.Sagas.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public abstract class SagaOrchestrator<TSagaLocator, TRequest>(
        IResolver resolver,
        ISagaStore sagaStore,
        TSagaLocator sagaLocator) : SagaTransactionAssistant<TRequest>, ISagaOrchestrator<TSagaLocator, TRequest>
        where TSagaLocator : ISagaLocator
        where TRequest : IRequest<SagaTransactionResult>
    {
        private readonly IResolver _resolver = resolver;
        private readonly ISagaStore _sagaStore = sagaStore;
        private readonly TSagaLocator _sagaLocator = sagaLocator;
        private TRequest _initialTransaction;
        private List<SagaActivityStore> _activities;

        public ISagaOrchestrator<TSagaLocator, TRequest> PrepareOrchestrator(TRequest initialTransaction)
        {
            _initialTransaction = initialTransaction;
            _activities = [];

            return this;
        }

        public ISagaOrchestrator<TSagaLocator, TRequest> Activity<TActivity>(string name) where TActivity : ISagaActivity
        {
            if (_activities is not null)
            {
                SagaActivityStore existingSagaActiveStore = _activities.Where(x => x.ActivityName.IsEqual(name, true)).SingleOrDefault();

                if (existingSagaActiveStore is not null)
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

            if (existingActivity is null)
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
            Guid correlationId = Guid.Create();

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
            IList<ISagaActivity> compensations = [];
            SagaOrchestratorContext context = new(correlationId, _initialTransaction);
            bool success = false;
            List<SagaTrail> trail = [];

            foreach (SagaActivityStore activity in _activities)
            {
                ISagaActivity currentActivity = _resolver.CreateInstance<ISagaActivity>(activity.ActivityType);
                ISagaActivity compensation = null;

                if (activity.CompensationType is not null)
                {
                    compensation = _resolver.CreateInstance<ISagaActivity>(activity.CompensationType);

                    if (compensation is not null)
                    {
                        compensations.Add(compensation);
                    }
                }

                SagaTransactionMeta sagaTransactionMeta = await GetSagaTransaction(
                    _sagaStore,
                    sagaId,
                    typeof(TRequest).Name,
                    activity.ActivityName,
                    cancellationToken)
                    .ConfigureAwait(false);
                SagaTrail trailItem = await ProcessActivity(
                    sagaTransactionMeta,
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
                    IEnumerable<ISagaActivity> compensationActivities = compensations.Reverse();

                    foreach (ISagaActivity compensationActivity in compensationActivities)
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

        private async Task<SagaTrail> ProcessActivity(
            SagaTransactionMeta sagaTransactionMeta,
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
                    success = sagaTransactionResult is not null && sagaTransactionResult.Success;

                    if (!success)
                    {
                        if (sagaTransactionResult is not null && sagaTransactionResult.Errors.Any())
                        {
                            errors = sagaTransactionResult.Errors;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                errors = [new Error(ReservedErrorCode.SystemError, exception.Message)];
            }

            return new SagaTrail(sagaTransactionMeta.Step, success, errors);
        }
    }
}
