using CrystalSharp.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate
{
    [Table("ORDERDETAIL", Schema = "SYSTEM")]
    public class OrderDetail : Entity<int>
    {
        public int SaleOrderId { get; private set; }
        public SaleOrder SaleOrder { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public static OrderDetail Create(SaleOrder saleOrder, string name, int quantity, decimal price)
        {
            OrderDetail orderDetail = new() { SaleOrderId = saleOrder.Id, SaleOrder = saleOrder, Name = name, Quantity = quantity, Price = price };

            return orderDetail;
        }
    }
}
