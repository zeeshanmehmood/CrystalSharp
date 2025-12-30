using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate.Events;

namespace CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate
{
    public class Supplier : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public SupplierDetails SupplierDetails { get; private set; }

        private static void ValidateSupplier(Supplier supplier)
        {
            if (string.IsNullOrEmpty(supplier.Name))
            {
                supplier.ThrowDomainException("Supplier name is required.");
            }

            if (supplier.SupplierDetails is null)
            {
                supplier.ThrowDomainException("Supplier details are required.");
            }

            if (string.IsNullOrEmpty(supplier.SupplierDetails.Code))
            {
                supplier.ThrowDomainException("Supplier code is required.");
            }

            if (supplier.SupplierDetails.Code.Length > 3)
            {
                supplier.ThrowDomainException("Only three characters allowed for supplier code.");
            }

            if (string.IsNullOrEmpty(supplier.SupplierDetails.Email))
            {
                supplier.ThrowDomainException("Supplier email is required.");
            }
        }

        public static string GetSampleSupplierName()
        {
            string name = "Sample";

            return name;
        }

        public static string GetTestSupplierName()
        {
            string name = "TEST";

            return name;
        }

        public static Supplier Create(string name, SupplierDetails supplierDetails)
        {
            Supplier supplier = new() { Name = name, SupplierDetails = supplierDetails };

            ValidateSupplier(supplier);

            supplier.Raise(new SupplierCreatedDomainEvent(supplier.GlobalUId, supplier.Name, supplier.SupplierDetails));

            return supplier;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateSupplier(this);

            Raise(new SupplierNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeDetails(SupplierDetails supplierDetails)
        {
            SupplierDetails = supplierDetails;

            ValidateSupplier(this);

            Raise(new SupplierDetailsChangedDomainEvent(GlobalUId, SupplierDetails));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new SupplierDeletedDomainEvent(GlobalUId, Name, SupplierDetails));
        }

        private void Apply(SupplierCreatedDomainEvent @event)
        {
            Name = @event.Name;
            SupplierDetails = @event.SupplierDetails;
        }

        private void Apply(SupplierNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(SupplierDetailsChangedDomainEvent @event)
        {
            SupplierDetails = @event.SupplierDetails;
        }

        private void Apply(SupplierDeletedDomainEvent @event)
        {
            //
        }
    }
}
