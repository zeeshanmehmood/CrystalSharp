using CrystalSharp.Envoy.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Envoy
{
    public interface INotificationHandler<in TNotification>
        where TNotification : INotificationMessage
    {
        Task Handle(TNotification notification, CancellationToken cancellationToken = default);
    }
}
