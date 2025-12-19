using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public interface ISagaActivity
    {
        Task<SagaTransactionResult> Execute(SagaOrchestratorContext context, CancellationToken cancellationToken = default);
    }
}
