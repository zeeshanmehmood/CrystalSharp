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

using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate.Events;
using CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate.Snapshots;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate
{
    public class Product : SnapshotAggregateRoot<int, ProductSnapshot>
    {
        public string Name { get; private set; }
        public ProductInfo ProductInfo { get; private set; }

        private static void ValidateProduct(Product product)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                product.ThrowDomainException("Product name is required.");
            }

            if (string.IsNullOrEmpty(product.ProductInfo.Sku))
            {
                product.ThrowDomainException("Product SKU is required.");
            }

            if (product.ProductInfo.Price <= 0)
            {
                product.ThrowDomainException("Product price is required.");
            }
        }

        public static Product Create(string name, ProductInfo productInfo)
        {
            Product product = new Product { Name = name, ProductInfo = productInfo };

            ValidateProduct(product);

            product.Raise(new ProductCreatedDomainEvent(product.GlobalUId, product.Name, product.ProductInfo));

            return product;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateProduct(this);

            Raise(new ProductNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeProductInfo(ProductInfo productInfo)
        {
            ProductInfo = productInfo;

            ValidateProduct(this);

            Raise(new ProductInfoChangedDomainEvent(GlobalUId, ProductInfo));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new ProductDeletedDomainEvent(GlobalUId));
        }

        private void Apply(ProductCreatedDomainEvent @event)
        {
            Name = @event.Name;
            ProductInfo = @event.ProductInfo;
        }

        private void Apply(ProductNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(ProductInfoChangedDomainEvent @event)
        {
            ProductInfo = @event.ProductInfo;
        }

        private void Apply(ProductDeletedDomainEvent @event)
        {
            //
        }
    }
}
