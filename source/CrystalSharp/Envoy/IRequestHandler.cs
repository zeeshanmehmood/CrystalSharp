using CrystalSharp.Envoy.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Envoy
{
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
