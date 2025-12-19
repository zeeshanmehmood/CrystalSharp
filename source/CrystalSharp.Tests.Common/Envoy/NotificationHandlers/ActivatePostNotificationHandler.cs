using CrystalSharp.Envoy;
using CrystalSharp.Tests.Common.Envoy.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Envoy.NotificationHandlers
{
    public class ActivatePostNotificationHandler : INotificationHandler<PostCreatedNotification>
    {
        public async Task Handle(PostCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            notification.Status += "Active.";
        }
    }
}
