using CrystalSharp.Envoy;
using CrystalSharp.Envoy.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Handlers
{
    public abstract class NotificationHandler<TNotification> : INotificationHandler<TNotification> where TNotification : INotificationMessage
    {
        public abstract Task Handle(TNotification notification, CancellationToken cancellationToken = default);
    }
}
