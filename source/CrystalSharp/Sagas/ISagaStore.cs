using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public interface ISagaStore
    {
        Task<SagaTransactionMeta> Get(string correlationId, CancellationToken cancellationToken = default);
        Task Upsert(SagaTransactionMeta sagaTransactionMeta, CancellationToken cancellationToken = default);
    }
}
