using CrystalSharp.Envoy.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Envoy
{
    public interface IEnvoy
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotificationMessage;
    }
}
