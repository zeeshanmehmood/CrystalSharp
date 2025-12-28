using CrystalSharp.Domain;

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.InvoiceAggregate
{
    public class LineItem : Entity<int>
    {
        public int InvoiceId { get; private set; }
        public Invoice Invoice { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public static LineItem Create(Invoice invoice, string name, int quantity, decimal price)
        {
            LineItem lineItem = new() { InvoiceId = invoice.Id, Invoice = invoice, Name = name, Quantity = quantity, Price = price };

            return lineItem;
        }
    }
}
