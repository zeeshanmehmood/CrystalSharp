using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Tests.Common.Envoy.Notifications
{
    public class ProductCreatedNotification : INotificationMessage
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
}
