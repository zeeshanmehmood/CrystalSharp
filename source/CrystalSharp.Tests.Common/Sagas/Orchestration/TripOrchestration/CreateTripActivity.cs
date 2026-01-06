using CrystalSharp.Sagas;
using CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Sagas.Orchestration.TripOrchestration
{
    public class CreateTripActivity(IInMemoryDataContext dataContext) : ISagaActivity
    {
        private readonly IInMemoryDataContext _dataContext = dataContext;

        public async Task<SagaTransactionResult> Execute(SagaOrchestratorContext sagaContext, CancellationToken cancellationToken = default)
        {
            PlanTripTransaction transaction = (PlanTripTransaction)sagaContext.Data;
            Trip trip = Trip.Create(transaction.Name, sagaContext.CorrelationId);

            await _dataContext.Trip.AddAsync(trip, cancellationToken).ConfigureAwait(false);
            await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new SagaTransactionResult(sagaContext.CorrelationId, true);
        }
    }
}
