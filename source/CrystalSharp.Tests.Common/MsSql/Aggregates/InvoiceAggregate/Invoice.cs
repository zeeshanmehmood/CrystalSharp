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

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.InvoiceAggregate
{
    public class Invoice : AggregateRoot<int>
    {
        public string Code { get; private set; }
        public decimal TotalAmount { get; private set; }
        public ICollection<LineItem> LineItems { get; private set; }

        private static void ValidateInvoice(Invoice invoice)
        {
            if (string.IsNullOrEmpty(invoice.Code))
            {
                invoice.ThrowDomainException("Invoice code is required.");
            }

            if (invoice.LineItems == null || invoice.LineItems.Count == 0)
            {
                invoice.ThrowDomainException("Cannot create invoice without line items.");
            }
        }

        private void ValidateLineItem(LineItem lineItem)
        {
            if (string.IsNullOrEmpty(lineItem.Name))
            {
                ThrowDomainException("Line item name is required.");
            }

            if (lineItem.Quantity < 1)
            {
                ThrowDomainException("Line item quantity must be greater than 0.");
            }

            if (lineItem.Price < 1)
            {
                ThrowDomainException("Line item price must be greater than 0.");
            }
        }

        public static Invoice Create(string code)
        {
            Invoice invoice = new() { Code = code };

            return invoice;
        }

        public void AddLineItem(string name, int quantity, decimal price)
        {
            LineItems ??= new List<LineItem>();
            LineItem lineItem = LineItem.Create(this, name, quantity, price);

            ValidateLineItem(lineItem);
            LineItems.Add(lineItem);

            TotalAmount = CalculateAmount();
        }

        public void Validate()
        {
            ValidateInvoice(this);
        }

        private decimal CalculateAmount()
        {
            if (LineItems == null || LineItems.Count == 0) return 0;

            return LineItems.Sum(x => x.Quantity * x.Price);
        }
    }
}
