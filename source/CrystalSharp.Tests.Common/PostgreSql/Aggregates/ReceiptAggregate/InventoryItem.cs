using CrystalSharp.Domain;

namespace CrystalSharp.Tests.Common.PostgreSql.Aggregates.ReceiptAggregate
{
    public class InventoryItem : Entity<int>
    {
        public int ReceiptId { get; private set; }
        public Receipt Receipt { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public static InventoryItem Create(Receipt receipt, string name, int quantity, decimal price)
        {
            InventoryItem inventoryItem = new() { ReceiptId = receipt.Id, Receipt = receipt, Name = name, Quantity = quantity, Price = price };

            return inventoryItem;
        }
    }
}
