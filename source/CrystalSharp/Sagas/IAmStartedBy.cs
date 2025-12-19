using CrystalSharp.Envoy;
using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Sagas
{
    public interface IAmStartedBy<TRequest> : IRequestHandler<TRequest, SagaTransactionResult>
        where TRequest : IRequest<SagaTransactionResult>
    {
        //
    }
}
