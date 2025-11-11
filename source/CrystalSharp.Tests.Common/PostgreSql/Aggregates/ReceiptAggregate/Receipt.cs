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

        public static Receipt Create(string code)
        {
            Receipt receipt = new() { Code = code };

            return receipt;
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
