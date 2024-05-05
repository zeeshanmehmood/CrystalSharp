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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MsSql.ReadModels;

namespace CrystalSharp.MsSql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MsSqlReadModelPersistenceIntegration)]
    public class MsSqlReadModelPersistenceTests : IClassFixture<MsSqlReadModelStoreTestFixture>
    {
        private readonly MsSqlReadModelStoreTestFixture _testFixture;

        public MsSqlReadModelPersistenceTests(MsSqlReadModelStoreTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel virtualShop = VirtualShopReadModel.Create("Hair Dryer", 55);

            // Act
            bool result = await sut.Store(virtualShop).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel electricTeaMaker = VirtualShopReadModel.Create("Electric Tea Maker", 200);
            VirtualShopReadModel roomCooler = VirtualShopReadModel.Create("Room Cooler", 700);
            VirtualShopReadModel coffeeMaker = VirtualShopReadModel.Create("Coffee Maker", 400);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { electricTeaMaker, roomCooler, coffeeMaker };

            // Act
            bool result = await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Oven", 600);
            await sut.Store(product).ConfigureAwait(false);
            product.Change("Projector", 350);

            // Act
            bool result = await sut.Update(product).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Doorbell Camera", 100);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<VirtualShopReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Fancy Keyboard", 150);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<VirtualShopReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Wireless Headset", 50);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<VirtualShopReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Cutlery Set", 150);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<VirtualShopReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel dvdPlayer = VirtualShopReadModel.Create("DVD Player", 150);
            VirtualShopReadModel readingLamp = VirtualShopReadModel.Create("Reading Lamp", 50);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { dvdPlayer, readingLamp };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = products.Select(x => x.Id);

            // Act
            bool result = await sut.BulkDelete<VirtualShopReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel coolingBeverageRefrigerator = VirtualShopReadModel.Create("Cooling Beverage Refrigerator", 600);
            VirtualShopReadModel dishwasher = VirtualShopReadModel.Create("Dishwasher", 400);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { coolingBeverageRefrigerator, dishwasher };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkDelete<VirtualShopReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel electricHeater = VirtualShopReadModel.Create("Electric Heater", 70);
            VirtualShopReadModel foodSlicer = VirtualShopReadModel.Create("Food Slicer", 115);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { electricHeater, foodSlicer };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = products.Select(x => x.Id);

            // Act
            bool result = await sut.BulkSoftDelete<VirtualShopReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel miniFridge = VirtualShopReadModel.Create("Mini Fridge", 250);
            VirtualShopReadModel riceCooker = VirtualShopReadModel.Create("Rice Cooker", 100);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { miniFridge, riceCooker };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkSoftDelete<VirtualShopReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Video Recorder", 200);
            await sut.Store<VirtualShopReadModel>(product).ConfigureAwait(false);
            await sut.SoftDelete<VirtualShopReadModel>(product.Id).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<VirtualShopReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Cordless Vacuum Cleaner", 230);
            await sut.Store<VirtualShopReadModel>(product).ConfigureAwait(false);
            await sut.SoftDelete<VirtualShopReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<VirtualShopReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel gamingTvStand = VirtualShopReadModel.Create("Gaming TV Stand", 160);
            VirtualShopReadModel vehicleCharger = VirtualShopReadModel.Create("Vehicle Charger", 15);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { gamingTvStand, vehicleCharger };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = products.Select(x => x.Id);
            await sut.BulkSoftDelete<VirtualShopReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<int> recordsToRestore = products.Select(x => x.Id);

            // Act
            bool result = await sut.BulkRestore<VirtualShopReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel portableDvdPlayer = VirtualShopReadModel.Create("Portable DVD Player", 170);
            VirtualShopReadModel gamingMouse = VirtualShopReadModel.Create("Wireless Optical Gaming Mouse", 150);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { portableDvdPlayer, gamingMouse };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = products.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<VirtualShopReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = products.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkRestore<VirtualShopReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Electric Guitar", 500);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            long result = await sut.Count<VirtualShopReadModel>().ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Electric Fan", 150);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            VirtualShopReadModel result = await sut.Find<VirtualShopReadModel>(product.Id).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(product.Id);
                result.GlobalUId.Should().Be(product.GlobalUId);
                result.Product.Should().Be(product.Product);
                result.Price.Should().Be(product.Price);
            }
        }

        [Fact]
        public async Task Find_read_model_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel product = VirtualShopReadModel.Create("Emergency Light", 130);
            await sut.Store(product).ConfigureAwait(false);

            // Act
            VirtualShopReadModel result = await sut.Find<VirtualShopReadModel>(product.GlobalUId).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(product.Id);
                result.GlobalUId.Should().Be(product.GlobalUId);
                result.Product.Should().Be(product.Product);
                result.Price.Should().Be(product.Price);
            }
        }

        [Fact]
        public async Task Get_all_records()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            VirtualShopReadModel washingMachine = VirtualShopReadModel.Create("Washing Machine", 1000);
            VirtualShopReadModel electricShaver = VirtualShopReadModel.Create("Electric Shaver", 50);
            await sut.Store(washingMachine).ConfigureAwait(false);
            await sut.Store(electricShaver).ConfigureAwait(false);

            // Act
            PagedResult<VirtualShopReadModel> result = await sut.Get<VirtualShopReadModel>(0, 10).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PageCount.Should().BeGreaterThan(0);
                result.PageSize.Should().BeGreaterThan(0);
                result.RowCount.Should().BeGreaterThan(0);
                result.Data.Count().Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public async Task Search()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            Expression<Func<VirtualShopReadModel, bool>> predicate = x => x.Product.ToLower().Contains("bulb");
            VirtualShopReadModel tableLamp = VirtualShopReadModel.Create("Table Lamp", 350);
            VirtualShopReadModel ledBulb = VirtualShopReadModel.Create("LED Bulb", 50);
            VirtualShopReadModel fancyBulb = VirtualShopReadModel.Create("Fancy Bulb", 70);
            IList<VirtualShopReadModel> products = new List<VirtualShopReadModel> { tableLamp, ledBulb, fancyBulb };
            await sut.BulkStore(products.AsEnumerable()).ConfigureAwait(false);

            // Act
            PagedResult<VirtualShopReadModel> result = await sut.Get<VirtualShopReadModel>(0, 10, predicate, RecordMode.Active, "Price", DataSortMode.Ascending, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PageCount.Should().BeGreaterThan(0);
                result.PageSize.Should().BeGreaterThan(0);
                result.RowCount.Should().BeGreaterThan(0);
                result.Data.Count().Should().BeGreaterThan(0);
                result.Data.SingleOrDefault(x => x.GlobalUId == ledBulb.GlobalUId).Product.Should().Be(ledBulb.Product);
                result.Data.SingleOrDefault(x => x.GlobalUId == fancyBulb.GlobalUId).Product.Should().Be(fancyBulb.Product);
            }
        }
    }
}
