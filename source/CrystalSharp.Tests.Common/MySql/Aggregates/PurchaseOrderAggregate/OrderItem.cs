using CrystalSharp.Domain;

namespace CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate
{
    public class OrderItem : Entity<int>
    {
        public int PurchaseOrderId { get; private set; }
        public PurchaseOrder PurchaseOrder { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public static OrderItem Create(PurchaseOrder purchaseOrder, string name, int quantity, decimal price)
        {
            OrderItem orderItem = new() { PurchaseOrderId = purchaseOrder.Id, PurchaseOrder = purchaseOrder, Name = name, Quantity = quantity, Price = price };

            return orderItem;
        }
    }
}
