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
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MySql.ReadModels;

namespace CrystalSharp.MySql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MySqlReadModelPersistenceIntegration)]
    public class MySqlReadModelPersistenceTests : IClassFixture<MySqlReadModelStoreTestFixture>
    {
        private readonly MySqlReadModelStoreTestFixture _testFixture;

        public MySqlReadModelPersistenceTests(MySqlReadModelStoreTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Falcon", "FLC");

            // Act
            bool result = await sut.Store(supplier).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel cosmos = SupplierReadModel.Create("Cosmos", "CMS");
            SupplierReadModel threeStar = SupplierReadModel.Create("Three Star", "TSR");
            SupplierReadModel sunshine = SupplierReadModel.Create("Sunshine", "SSH");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { cosmos, threeStar, sunshine };

            // Act
            bool result = await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("QQ North", "QQN");
            await sut.Store(supplier).ConfigureAwait(false);
            supplier.Change("Global", "GBL");

            // Act
            bool result = await sut.Update(supplier).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("South Line", "SLN");
            await sut.Store(supplier).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<SupplierReadModel>(supplier.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Speedy", "SPD");
            await sut.Store(supplier).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<SupplierReadModel>(supplier.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Five Star", "FS");
            await sut.Store(supplier).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<SupplierReadModel>(supplier.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Skyline", "SKN");
            await sut.Store(supplier).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<SupplierReadModel>(supplier.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel fastLine = SupplierReadModel.Create("Fast Line", "FL");
            SupplierReadModel master = SupplierReadModel.Create("Master", "MS");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { fastLine, master };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = suppliers.Select(x => x.Id);

            // Act
            bool result = await sut.BulkDelete<SupplierReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel cloud = SupplierReadModel.Create("Cloud", "CLD");
            SupplierReadModel oneWay = SupplierReadModel.Create("One Way", "OW");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { cloud, oneWay };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = suppliers.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkDelete<SupplierReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel jupiter = SupplierReadModel.Create("Jupiter", "JPT");
            SupplierReadModel stoneMark = SupplierReadModel.Create("Stone Mark", "STM");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { jupiter, stoneMark };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = suppliers.Select(x => x.Id);

            // Act
            bool result = await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel glassNGlass = SupplierReadModel.Create("Glass n Glass", "GNG");
            SupplierReadModel woodDeal = SupplierReadModel.Create("Wood Deal", "WDD");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { glassNGlass, woodDeal };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = suppliers.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Digital Star", "DS");
            await sut.Store<SupplierReadModel>(supplier).ConfigureAwait(false);
            await sut.SoftDelete<SupplierReadModel>(supplier.Id).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<SupplierReadModel>(supplier.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Rockstone", "RST");
            await sut.Store<SupplierReadModel>(supplier).ConfigureAwait(false);
            await sut.SoftDelete<SupplierReadModel>(supplier.GlobalUId).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<SupplierReadModel>(supplier.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel fastTrack = SupplierReadModel.Create("Fast Track", "FT");
            SupplierReadModel fancyWear = SupplierReadModel.Create("Fancy Wear", "FW");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { fastTrack, fancyWear };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = suppliers.Select(x => x.Id);
            await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<int> recordsToRestore = suppliers.Select(x => x.Id);

            // Act
            bool result = await sut.BulkRestore<SupplierReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel skyTech = SupplierReadModel.Create("Sky Tech", "SKT");
            SupplierReadModel electroMagic = SupplierReadModel.Create("Electro Magic", "EM");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { skyTech, electroMagic };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = suppliers.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = suppliers.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkRestore<SupplierReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Sweet Cleaner", "SC");
            await sut.Store(supplier).ConfigureAwait(false);

            // Act
            long result = await sut.Count<SupplierReadModel>().ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Digital Stereo", "DSR");
            await sut.Store(supplier).ConfigureAwait(false);

            // Act
            SupplierReadModel result = await sut.Find<SupplierReadModel>(supplier.Id).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(supplier.Id);
                result.GlobalUId.Should().Be(supplier.GlobalUId);
                result.Name.Should().Be(supplier.Name);
                result.Code.Should().Be(supplier.Code);
            }
        }

        [Fact]
        public async Task Find_read_model_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Smart Steel", "SST");
            await sut.Store(supplier).ConfigureAwait(false);

            // Act
            SupplierReadModel result = await sut.Find<SupplierReadModel>(supplier.GlobalUId).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(supplier.Id);
                result.GlobalUId.Should().Be(supplier.GlobalUId);
                result.Name.Should().Be(supplier.Name);
                result.Code.Should().Be(supplier.Code);
            }
        }

        [Fact]
        public async Task Filter_read_model_by_predicate()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel crystalElectronics = SupplierReadModel.Create("Crystal Electronics", "CE");
            SupplierReadModel crystalWoods = SupplierReadModel.Create("Crystal Woods", "CW");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { crystalElectronics, crystalWoods };
            await sut.BulkStore<SupplierReadModel>(suppliers, CancellationToken.None).ConfigureAwait(false);
            Expression<Func<SupplierReadModel, bool>> predicate = x => x.Name.StartsWith("Crystal") && x.EntityStatus == EntityStatus.Active;

            // Act
            IQueryable<SupplierReadModel> result = await sut.Filter(predicate).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Count().Should().Be(2);
                result.SingleOrDefault(x => x.Name == crystalElectronics.Name).Should().NotBeNull();
                result.SingleOrDefault(x => x.Name == crystalWoods.Name).Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_all_records()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel smartAluminium = SupplierReadModel.Create("Smart Aluminium", "SAL");
            SupplierReadModel smartStone = SupplierReadModel.Create("Smart Stone", "STO");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { smartAluminium, smartStone };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);

            // Act
            PagedResult<SupplierReadModel> result = await sut.Get<SupplierReadModel>(0, 10).ConfigureAwait(false);

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
            Expression<Func<SupplierReadModel, bool>> predicate = x => x.Name.ToLower().Contains("wood");
            SupplierReadModel microWood = SupplierReadModel.Create("Micro Wood", "MWD");
            SupplierReadModel microSteel = SupplierReadModel.Create("Micro Steel", "MCS");
            SupplierReadModel crownWood = SupplierReadModel.Create("Crown Wood", "CRW");
            IList<SupplierReadModel> suppliers = new List<SupplierReadModel> { microWood, microSteel, crownWood };
            await sut.BulkStore(suppliers.AsEnumerable()).ConfigureAwait(false);

            // Act
            PagedResult<SupplierReadModel> result = await sut.Get<SupplierReadModel>(0, 10, predicate, false, RecordMode.Active, "Name", DataSortMode.Descending, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PageCount.Should().BeGreaterThan(0);
                result.PageSize.Should().BeGreaterThan(0);
                result.RowCount.Should().BeGreaterThan(0);
                result.Data.Count().Should().BeGreaterThan(0);
                result.Data.SingleOrDefault(x => x.GlobalUId == microWood.GlobalUId).Name.Should().Be(microWood.Name);
                result.Data.SingleOrDefault(x => x.GlobalUId == crownWood.GlobalUId).Name.Should().Be(crownWood.Name);
            }
        }
    }
}
