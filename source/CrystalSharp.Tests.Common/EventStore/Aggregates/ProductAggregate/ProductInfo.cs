using CrystalSharp.Domain;
using System;
using System.Collections.Generic;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate
{
    public class ProductInfo(string sku, decimal price) : ValueObject
    {
        public string Sku { get; private set; } = sku;
        public decimal Price { get; private set; } = price;

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Sku;
            yield return Price;
        }
    }
}
