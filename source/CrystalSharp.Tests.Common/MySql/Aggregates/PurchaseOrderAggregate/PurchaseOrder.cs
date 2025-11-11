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
using System.Linq;
using CrystalSharp.Domain;

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

        public static PurchaseOrder Create(string code)
        {
            PurchaseOrder purchaseOrder = new() { Code = code };

            return purchaseOrder;
        }

        public void AddOrderItem(string name, int quantity, decimal price)
        {
            OrderItems ??= new List<OrderItem>();
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
