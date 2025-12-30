using CrystalSharp.Domain;
using System.Collections.Generic;
using System.Linq;

namespace CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate
{
    public class PurchaseOrder : AggregateRoot<int>
    {
        public string Code { get; private set; }
        public decimal TotalAmount { get; private set; }
        public ICollection<OrderItem> OrderItems { get; private set; }

        private static void ValidatePurchaseOrder(PurchaseOrder purchaseOrder)
        {
            if (string.IsNullOrEmpty(purchaseOrder.Code))
            {
                purchaseOrder.ThrowDomainException("Purcahse order code is required.");
            }

            if (purchaseOrder.OrderItems == null || purchaseOrder.OrderItems.Count == 0)
            {
                purchaseOrder.ThrowDomainException("Cannot create purchase order without order items.");
            }
        }

        public void ValidateOrderItem(OrderItem orderItem)
        {
            if (string.IsNullOrEmpty(orderItem.Name))
            {
                ThrowDomainException("Order item name is required.");
            }

            if (orderItem.Quantity < 1)
            {
                ThrowDomainException("Order item quantity must be greater than 0.");
            }

            if (orderItem.Price < 1)
            {
                ThrowDomainException("Order item price must be greater than 0.");
            }
        }

        public static string GetSamplePurchaseOrderCode()
        {
            string code = "Sample";

            return code;
        }

        public static string GetTestPurchaseOrderCode()
        {
            string code = "TEST";

            return code;
        }

        public static PurchaseOrder Create(string code)
        {
            PurchaseOrder purchaseOrder = new() { Code = code };

            return purchaseOrder;
        }

        public void ChangeCode(string code)
        {
            Code = code;
        }

        public void AddOrderItem(string name, int quantity, decimal price)
        {
            OrderItems ??= [];
            OrderItem orderItem = OrderItem.Create(this, name, quantity, price);

            ValidateOrderItem(orderItem);
            OrderItems.Add(orderItem);

            TotalAmount = CalculateAmount();
        }

        public void Validate()
        {
            ValidatePurchaseOrder(this);
        }

        private decimal CalculateAmount()
        {
            if (OrderItems == null || OrderItems.Count == 0) return 0;

            return OrderItems.Sum(x => x.Quantity * x.Price);
        }
    }
}
