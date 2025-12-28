using CrystalSharp.Domain;
using System.Collections.Generic;
using System.Linq;

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

        public static string GetSampleInvoiceCode()
        {
            string code = "Sample";

            return code;
        }

        public static string GetTestInvoiceCode()
        {
            string code = "TEST";

            return code;
        }

        public static Invoice Create(string code)
        {
            Invoice invoice = new() { Code = code };

            return invoice;
        }

        public void ChangeCode(string code)
        {
            Code = code;
        }

        public void AddLineItem(string name, int quantity, decimal price)
        {
            LineItems ??= [];
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
