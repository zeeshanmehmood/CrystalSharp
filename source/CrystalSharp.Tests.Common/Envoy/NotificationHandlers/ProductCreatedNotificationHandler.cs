using CrystalSharp.Envoy;
using CrystalSharp.Tests.Common.Envoy.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Envoy.NotificationHandlers
{
    public class ProductCreatedNotificationHandler : INotificationHandler<ProductCreatedNotification>
    {
        public async Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            notification.Description = $"{notification.Name}: {notification.Price}";
        }
    }
}
