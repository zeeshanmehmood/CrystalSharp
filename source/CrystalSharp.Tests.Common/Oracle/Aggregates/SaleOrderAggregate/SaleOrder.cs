// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using CrystalSharp.Domain;

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

            if (saleOrder.Orders == null || saleOrder.Orders.Count == 0)
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

        public static SaleOrder Create(string code)
        {
            SaleOrder saleOrder = new() { Code = code };

            return saleOrder;
        }

        public void AddOrderDetail(string name, int quantity, decimal price)
        {
            Orders ??= new List<OrderDetail>();
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
