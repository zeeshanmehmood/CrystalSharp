using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Tests.Common.Envoy.Notifications
{
    public class PostCreatedNotification : INotificationMessage
    {
        public string Title { get; set; }
        public string Status { get; set; }
    }
}
