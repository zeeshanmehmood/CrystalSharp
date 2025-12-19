using CrystalSharp.Application.Handlers;
using CrystalSharp.Tests.Common.Application.NotificationExecution.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Application.NotificationExecution.NotificationHandlers
{
    public class OrderDispatchedSmsNotificationHandler : NotificationHandler<OrderDispatchedNotification>
    {
        public override async Task Handle(OrderDispatchedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            notification.Status += "SMS sent.";
        }
    }
}
