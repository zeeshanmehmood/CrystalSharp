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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Elasticsearch.ReadModels;

namespace CrystalSharp.ReadModelStores.Elasticsearch.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.ElasticsearchReadModelPersistenceIntegration)]
    public class ElasticsearchReadModelPersistenceTests : IClassFixture<ElasticsearchReadModelPersistenceTestFixture>
    {
        private readonly ElasticsearchReadModelPersistenceTestFixture _testFixture;

        public ElasticsearchReadModelPersistenceTests(ElasticsearchReadModelPersistenceTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Electric Tea Maker", "Handy tea maker.", 50);

            // Act
            bool result = await sut.Store(product).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel roomCooler = ProductReadModel.Create("Room Cooler", "Get relaxed in summer with room cooler.", 400);
            ProductReadModel homeTools = ProductReadModel.Create("Home Tools", "Tools for everyday use.", 100);
            ProductReadModel coffeeMaker = ProductReadModel.Create("Coffee Maker", "Instant coffee maker.", 70);
            IList<ProductReadModel> products = new List<ProductReadModel> { roomCooler, homeTools, coffeeMaker };

            // Act
            bool result = await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Oven", "An easy to use oven", 200);
            await sut.Store(product).ConfigureAwait(false);
            product.Change("Projector", "Projector for your daily meetings.", 150, true, DateTime.Now.AddDays(5));

            // Act
            bool result = await sut.Update(product).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Doorbell Camera", "Compact doorbell camera for your home security.", 70);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<ProductReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Fancy Keyboard", "Fancy Keyboard for kids.", 150);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<ProductReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Wireless Headset", "Wireless Headset with good sound quality.", 50);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<ProductReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Cutlery Set", "Metallic cutlery set.", 150);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<ProductReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel dvdPlayer = ProductReadModel.Create("DVD Player", "DVD Player for home entertainment.", 150);
            ProductReadModel readingLamp = ProductReadModel.Create("Reading Lamp", "An easy to use reading lamp for study.", 30);
            IList<ProductReadModel> products = new List<ProductReadModel> { dvdPlayer, readingLamp };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.Id);

            // Act
            bool result = await sut.BulkDelete<ProductReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel coolingBeverageRefrigerator = ProductReadModel.Create("Cooling Beverage Refrigerator", "Best for beverages.", 600);
            ProductReadModel dishwasher = ProductReadModel.Create("Dishwasher", "An easy to use dishwasher.", 400);
            IList<ProductReadModel> products = new List<ProductReadModel> { coolingBeverageRefrigerator, dishwasher };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkDelete<ProductReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel electricHeater = ProductReadModel.Create("Electric Heater", "Best for winter.", 70);
            ProductReadModel foodSlicer = ProductReadModel.Create("Food Slicer", "An easy to use food slicer.", 115);
            IList<ProductReadModel> products = new List<ProductReadModel> { electricHeater, foodSlicer };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.Id);

            // Act
            bool result = await sut.BulkSoftDelete<ProductReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel miniFridge = ProductReadModel.Create("Mini Fridge", "Best for room.", 250);
            ProductReadModel riceCooker = ProductReadModel.Create("Rice Cooker", "An easy to use rice cooker.", 100);
            IList<ProductReadModel> products = new List<ProductReadModel> { miniFridge, riceCooker };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkSoftDelete<ProductReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Video Recorder", "Record your best moments.", 200);
            await sut.Store<ProductReadModel>(product).ConfigureAwait(false);
            await sut.SoftDelete<ProductReadModel>(product.Id).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<ProductReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Cordless Vacuum Cleaner", "An easy to use vacuum cleaner.", 230);
            await sut.Store<ProductReadModel>(product).ConfigureAwait(false);
            await sut.SoftDelete<ProductReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<ProductReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel gamingTvStand = ProductReadModel.Create("Gaming TV Stand", "Organize your home entertainment.", 160);
            ProductReadModel vehicleCharger = ProductReadModel.Create("Vehicle Charger", "Keep your phone charged when you are travelling.", 15);
            IList<ProductReadModel> products = new List<ProductReadModel> { gamingTvStand, vehicleCharger };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.Id);
            await sut.BulkSoftDelete<ProductReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = products.Select(x => x.Id);

            // Act
            bool result = await sut.BulkRestore<ProductReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel portableDvdPlayer = ProductReadModel.Create("Portable DVD Player", "An easy to use and portable DVD player.", 170);
            ProductReadModel gamingMouse = ProductReadModel.Create("Wireless Optical Gaming Mouse", "Best for gamers.", 150);
            IList<ProductReadModel> products = new List<ProductReadModel> { portableDvdPlayer, gamingMouse };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<ProductReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = products.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkRestore<ProductReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Electric Guitar", "Learn or sing your favorite songs with electric guitar.", 200);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            long result = await sut.Count<ProductReadModel>().ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Electric Fan", "Rechargeable electric fan.", 100);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            ProductReadModel result = await sut.Find<ProductReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(product.Id);
                result.GlobalUId.Should().Be(product.GlobalUId);
                result.Name.Should().Be(product.Name);
                result.Description.Should().Be(product.Description);
                result.Price.Should().Be(product.Price);
                result.InStock.Should().Be(product.InStock);
                result.CreatedOn.Should().Be(product.CreatedOn);
            }
        }

        [Fact]
        public async Task Find_read_model_by_globaluid()
        {
            // Arrange
            IReadModelStore<Guid> sut = _testFixture.ReadModelStore;
            ProductReadModel product = ProductReadModel.Create("Emergency Light", "Rechargeable emergency light.", 50);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            ProductReadModel result = await sut.Find<ProductReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(product.Id);
                result.GlobalUId.Should().Be(product.GlobalUId);
                result.Name.Should().Be(product.Name);
                result.Description.Should().Be(product.Description);
                result.Price.Should().Be(product.Price);
                result.InStock.Should().Be(product.InStock);
                result.CreatedOn.Should().Be(product.CreatedOn);
            }
        }
    }
}
