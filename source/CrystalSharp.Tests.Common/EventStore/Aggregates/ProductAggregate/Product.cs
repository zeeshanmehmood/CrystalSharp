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
            Product product = new() { Name = name, ProductInfo = productInfo };

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
