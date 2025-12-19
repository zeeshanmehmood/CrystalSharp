using CrystalSharp.Envoy.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public interface ISagaOrchestrator<TSagaLocator, TRequest>
        where TSagaLocator : ISagaLocator
        where TRequest : IRequest<SagaTransactionResult>
    {
        ISagaOrchestrator<TSagaLocator, TRequest> PrepareOrchestrator(TRequest initialTransaction);
        ISagaOrchestrator<TSagaLocator, TRequest> Activity<TActivity>(string name) where TActivity : ISagaActivity;
        ISagaOrchestrator<TSagaLocator, TRequest> WithCompensation<TCompensation>(string name) where TCompensation : ISagaActivity;
        Task<SagaResult> Run(CancellationToken cancellationToken = default);
        Task<SagaResult> Run(Guid correlationId, CancellationToken cancellationToken = default);
    }
}
