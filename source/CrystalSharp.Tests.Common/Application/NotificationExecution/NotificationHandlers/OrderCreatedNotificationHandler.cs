using CrystalSharp.Application.Handlers;
using CrystalSharp.Tests.Common.Application.NotificationExecution.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Application.NotificationExecution.NotificationHandlers
{
    public class OrderCreatedNotificationHandler : NotificationHandler<OrderCreatedNotification>
    {
        public override async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            notification.Status = "Order created";
        }
    }
}
