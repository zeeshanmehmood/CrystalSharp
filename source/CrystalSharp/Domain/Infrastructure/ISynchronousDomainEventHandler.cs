using CrystalSharp.Envoy;
using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Domain.Infrastructure
{
    public interface ISynchronousDomainEventHandler<in TNotification> : INotificationHandler<TNotification>
        where TNotification : INotificationMessage
    {
        //
    }
}
