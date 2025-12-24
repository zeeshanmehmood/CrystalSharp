using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.EventStores.KurrentDb.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.KurrentDbIntegration)]
    public class KurrentDbPersistenceTests(KurrentDbTestFixture fixture) : IClassFixture<KurrentDbTestFixture>
    {
        private readonly KurrentDbTestFixture _testFixture = fixture;

        [Fact]
        public async Task Event_persisted()
        {
            // Arrange
            IAggregateEventStore<int> sut = _testFixture.EventStorePersistence;
            Product product = Product.Create("Laptop", new ProductInfo("LP300", 300));

            // Act
            await sut.Store(product, CancellationToken.None).ConfigureAwait(false);
            Product result = await sut.Get<Product>(product.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(result, options => options.IncludingNestedObjects());
        }

        [Fact]
        public async Task Event_version_increased()
        {
            // Arrange
            IAggregateEventStore<int> sut = _testFixture.EventStorePersistence;
            Product product = Product.Create("Juicer", new ProductInfo("JC50", 50));
            await sut.Store(product, CancellationToken.None).ConfigureAwait(false);
            Product existingProduct = await sut.Get<Product>(product.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Act
            existingProduct.ChangeName("Blender");
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            Product result = await sut.Get<Product>(existingProduct.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Version.Should().Be(1);
        }

        [Fact]
        public async Task Get_particular_event_by_version()
        {
            // Arrange
            IAggregateEventStore<int> sut = _testFixture.EventStorePersistence;
            Product product = Product.Create("Armchair", new ProductInfo("AC70", 270));
            await sut.Store(product, CancellationToken.None).ConfigureAwait(false);
            Product existingProduct = await sut.Get<Product>(product.GlobalUId, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeName("Desk Chair");
            existingProduct.ChangeProductInfo(new ProductInfo("DC20", 220));
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeName("Tulip Chair");
            existingProduct.ChangeProductInfo(new ProductInfo("TC10", 210));
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);

            // Act
            Product result = await sut.GetByVersion<Product>(existingProduct.GlobalUId, 2, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Version.Should().Be(2);
                result.Name.Should().BeNull();
                result.ProductInfo.Sku.Should().Be("DC20");
                result.ProductInfo.Price.Should().Be(220);
            }
        }

        [Fact]
        public async Task Read_from_snapshot()
        {
            // Arrange
            IAggregateEventStore<int> sut = _testFixture.EventStorePersistence;
            Product product = Product.Create("Shelf", new ProductInfo("SH45", 445));
            await sut.Store(product, CancellationToken.None).ConfigureAwait(false);
            Product existingProduct = await sut.Get<Product>(product.GlobalUId, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeName("Book Shelf");
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeProductInfo(new ProductInfo("BS35", 435));
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeName("Classic Shelf");
            existingProduct.ChangeProductInfo(new ProductInfo("CS25", 425));
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeName("Classic Style Shelf");
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeProductInfo(new ProductInfo("CS15", 415));
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeName("Old Style Shelf");
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeProductInfo(new ProductInfo("CS10", 410));
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);
            existingProduct.ChangeProductInfo(new ProductInfo("CS05", 405));
            await sut.Store(existingProduct, CancellationToken.None).ConfigureAwait(false);

            // Act
            Product result = await sut.Get<Product>(existingProduct.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Version.Should().Be(9);
                result.Name.Should().Be("Old Style Shelf");
                result.ProductInfo.Sku.Should().Be("CS05");
                result.ProductInfo.Price.Should().Be(405);
                result.SnapshotVersion.Should().Be(2);
            }
        }

        [Fact]
        public async Task Raise_negative_version_exception()
        {
            // Arrange
            IAggregateEventStore<int> sut = _testFixture.EventStorePersistence;
            Product product = Product.Create("LED Screen", new ProductInfo("LD55", 555));
            await sut.Store(product, CancellationToken.None).ConfigureAwait(false);

            // Act
            Func<Task<Product>> result = async () => await sut.GetByVersion<Product>(product.GlobalUId, -1, CancellationToken.None).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<EventStoreNegativeVersionException>();
        }

        [Fact]
        public async Task Raise_stream_not_found_exception_when_stream_is_deleted()
        {
            // Arrange
            IAggregateEventStore<int> sut = _testFixture.EventStorePersistence;
            Product product = Product.Create("Floor Lamp", new ProductInfo("FL55", 155));
            await sut.Store(product, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.Delete<Product>(product.GlobalUId, CancellationToken.None).ConfigureAwait(false);
            Func<Task<Product>> result = async () => await sut.Get<Product>(product.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<EventStoreStreamNotFoundException>();
        }

        [Fact]
        public async Task Raise_stream_not_found_exception()
        {
            // Arrange
            IAggregateEventStore<int> sut = _testFixture.EventStorePersistence;

            // Act
            Func<Task<Product>> result = async () => await sut.Get<Product>(Guid.Create(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<EventStoreStreamNotFoundException>();
        }
    }
}
