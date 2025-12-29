using CrystalSharp.Domain;
using System.Collections.Generic;
using System.Linq;

namespace CrystalSharp.Tests.Common.PostgreSql.Aggregates.ReceiptAggregate
{
    public class Receipt : AggregateRoot<int>
    {
        public string Code { get; private set; }
        public decimal TotalAmount { get; private set; }
        public ICollection<InventoryItem> InventoryItems { get; private set; }

        private static void ValidateReceipt(Receipt receipt)
        {
            if (string.IsNullOrEmpty(receipt.Code))
            {
                receipt.ThrowDomainException("Receipt code is required.");
            }

            if (receipt.InventoryItems == null || receipt.InventoryItems.Count == 0)
            {
                receipt.ThrowDomainException("Cannot create receipt without inventory items.");
            }
        }

        public void ValidateInventoryItem(InventoryItem inventoryItem)
        {
            if (string.IsNullOrEmpty(inventoryItem.Name))
            {
                ThrowDomainException("Inventory item name is required.");
            }

            if (inventoryItem.Quantity < 1)
            {
                ThrowDomainException("Inventory item quantity must be greater than 0.");
            }

            if (inventoryItem.Price < 1)
            {
                ThrowDomainException("Inventory item price must be greater than 0.");
            }
        }

        public static string GetSampleReceiptCode()
        {
            string code = "Sample";

            return code;
        }

        public static string GetTestReceiptCode()
        {
            string code = "TEST";

            return code;
        }

        public static Receipt Create(string code)
        {
            Receipt receipt = new() { Code = code };

            return receipt;
        }

        public void ChangeCode(string code)
        {
            Code = code;
        }

        public void AddInventoryItem(string name, int quantity, decimal price)
        {
            InventoryItems ??= new List<InventoryItem>();
            InventoryItem inventoryItem = InventoryItem.Create(this, name, quantity, price);

            ValidateInventoryItem(inventoryItem);
            InventoryItems.Add(inventoryItem);

            TotalAmount = CalculateAmount();
        }

        public void Validate()
        {
            ValidateReceipt(this);
        }

        private decimal CalculateAmount()
        {
            if (InventoryItems == null || InventoryItems.Count == 0) return 0;

            return InventoryItems.Sum(x => x.Quantity * x.Price);
        }
    }
}
