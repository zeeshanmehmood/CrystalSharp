using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public interface ISagaTransactionExecutor
    {
        Task<SagaTransactionResult> Execute(ISagaTransaction transaction, CancellationToken cancellationToken = default);
    }
}
