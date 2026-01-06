using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Sagas;
using CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Sagas.Orchestration.TripOrchestration
{
    public class CancelTripActivity(IInMemoryDataContext dataContext) : ISagaActivity
    {
        private readonly IInMemoryDataContext _dataContext = dataContext;

        public async Task<SagaTransactionResult> Execute(SagaOrchestratorContext sagaContext, CancellationToken cancellationToken = default)
        {
            PlanTripTransaction transaction = (PlanTripTransaction)sagaContext.Data;
            Trip trip = _dataContext.Trip.SingleOrDefault(x => x.EntityStatus == EntityStatus.Active && x.CorrelationId == sagaContext.CorrelationId);

            if (trip is null)
            {
                return SagaTransactionResult.WithError([new(ReservedErrorCode.SystemError, "Trip not found.")]);
            }

            trip.CancelTrip();
            await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new SagaTransactionResult(sagaContext.CorrelationId, true);
        }
    }
}
