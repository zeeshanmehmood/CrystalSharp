using CrystalSharp.Application;

namespace CrystalSharp.Tests.Common.Application.NotificationExecution.Notifications
{
    public class OrderCreatedNotification : INotification
    {
        public string Status { get; set; }
    }
}
