using CrystalSharp.Application;

namespace CrystalSharp.Tests.Common.Application.NotificationExecution.Notifications
{
    public class OrderDispatchedNotification : INotification
    {
        public string Status { get; set; }
    }
}
