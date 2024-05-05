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

using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate.Events;

namespace CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate
{
    public class Supplier : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public SupplierInfo SupplierInfo { get; private set; }

        private static void ValidateSupplier(Supplier supplier)
        {
            if (string.IsNullOrEmpty(supplier.Name))
            {
                supplier.ThrowDomainException("Supplier name is required.");
            }

            if (supplier.SupplierInfo == null)
            {
                supplier.ThrowDomainException("Supplier info is required.");
            }

            if (string.IsNullOrEmpty(supplier.SupplierInfo.Code))
            {
                supplier.ThrowDomainException("Supplier code is required.");
            }

            if (supplier.SupplierInfo.Code.Length > 3)
            {
                supplier.ThrowDomainException("Only three characters allowed for supplier code.");
            }

            if (string.IsNullOrEmpty(supplier.SupplierInfo.Email))
            {
                supplier.ThrowDomainException("Supplier email is required.");
            }
        }

        public static Supplier Create(string name, SupplierInfo supplierInfo)
        {
            Supplier supplier = new Supplier { Name = name, SupplierInfo = supplierInfo };

            ValidateSupplier(supplier);

            supplier.Raise(new SupplierCreatedDomainEvent(supplier.GlobalUId, supplier.Name, supplier.SupplierInfo));

            return supplier;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateSupplier(this);

            Raise(new SupplierNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeInfo(SupplierInfo supplierInfo)
        {
            SupplierInfo = supplierInfo;

            ValidateSupplier(this);

            Raise(new SupplierInfoChangedDomainEvent(GlobalUId, SupplierInfo));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new SupplierDeletedDomainEvent(GlobalUId, Name, SupplierInfo));
        }

        private void Apply(SupplierCreatedDomainEvent @event)
        {
            Name = @event.Name;
            SupplierInfo = @event.SupplierInfo;
        }

        private void Apply(SupplierNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(SupplierInfoChangedDomainEvent @event)
        {
            SupplierInfo = @event.SupplierInfo;
        }

        private void Apply(SupplierDeletedDomainEvent @event)
        {
            //
        }
    }
}
