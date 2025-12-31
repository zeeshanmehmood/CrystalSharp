using CrystalSharp.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate
{
    [Table("SALEORDER", Schema = "SYSTEM")]
    public class SaleOrder : AggregateRoot<int>
    {
        public string Code { get; private set; }
        public decimal TotalAmount { get; private set; }
        public ICollection<OrderDetail> Orders { get; private set; }

        private static void ValidateSaleOrder(SaleOrder saleOrder)
        {
            if (string.IsNullOrEmpty(saleOrder.Code))
            {
                saleOrder.ThrowDomainException("Sale order code is required.");
            }

            if (saleOrder.Orders is null || saleOrder.Orders.Count == 0)
            {
                saleOrder.ThrowDomainException("Cannot create sale order without order details.");
            }
        }

        private void ValidateOrderDetail(OrderDetail orderDetail)
        {
            if (string.IsNullOrEmpty(orderDetail.Name))
            {
                ThrowDomainException("Name is required.");
            }

            if (orderDetail.Quantity < 1)
            {
                ThrowDomainException("Quantity must be greater than 0.");
            }

            if (orderDetail.Price < 1)
            {
                ThrowDomainException("Price must be greater than 0.");
            }
        }

        public static string GetSampleSaleOrderCode()
        {
            string code = "Sample";

            return code;
        }

        public static string GetTestSaleOrderCode()
        {
            string code = "TEST";

            return code;
        }

        public static SaleOrder Create(string code)
        {
            SaleOrder saleOrder = new() { Code = code };

            return saleOrder;
        }

        public void ChangeCode(string code)
        {
            Code = code;
        }

        public void AddOrderDetail(string name, int quantity, decimal price)
        {
            Orders ??= [];
            OrderDetail orderDetail = OrderDetail.Create(this, name, quantity, price);

            ValidateOrderDetail(orderDetail);
            Orders.Add(orderDetail);
            TotalAmount = CalculateAmount();
        }

        public void Validate()
        {
            ValidateSaleOrder(this);
        }

        private decimal CalculateAmount()
        {
            if (Orders == null || Orders.Count == 0) return 0;

            return Orders.Sum(x => x.Quantity * x.Price);
        }
    }
}
