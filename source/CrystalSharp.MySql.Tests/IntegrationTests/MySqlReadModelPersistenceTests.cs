using CrystalSharp.Common.Exceptions;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MySql.ReadModels;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CrystalSharp.MySql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MySqlReadModelPersistenceIntegration)]
    public class MySqlReadModelPersistenceTests(MySqlReadModelStoreTestFixture fixture) : IClassFixture<MySqlReadModelStoreTestFixture>
    {
        private readonly MySqlReadModelStoreTestFixture _testFixture = fixture;

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Falcon", "FLC");

            // Act
            int result = await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel cosmos = SupplierReadModel.Create("Cosmos", "CMS");
            SupplierReadModel threeStar = SupplierReadModel.Create("Three Star", "TSR");
            SupplierReadModel sunshine = SupplierReadModel.Create("Sunshine", "SSH");
            IList<SupplierReadModel> suppliers = [cosmos, threeStar, sunshine];

            // Act
            int result = await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("QQ North", "QQN");
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);
            supplier.Change("Global", "GBL");

            // Act
            int result = await sut.Update(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("South Line", "SLN");
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<SupplierReadModel>(supplier.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Speedy", "SPD");
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<SupplierReadModel>(supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Five Star", "FS");
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<SupplierReadModel>(supplier.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Skyline", "SKN");
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<SupplierReadModel>(supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel fastLine = SupplierReadModel.Create("Fast Line", "FL");
            SupplierReadModel master = SupplierReadModel.Create("Master", "MS");
            IList<SupplierReadModel> suppliers = [fastLine, master];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = suppliers.Select(x => x.Id);

            // Act
            int result = await sut.BulkDelete<SupplierReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel cloud = SupplierReadModel.Create("Cloud", "CLD");
            SupplierReadModel oneWay = SupplierReadModel.Create("One Way", "OW");
            IList<SupplierReadModel> suppliers = [cloud, oneWay];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = suppliers.Select(x => x.GlobalUId);

            // Act
            Func<Task<int>> result = async () => await sut.BulkDelete<SupplierReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<FunctionalityNotAvailableException>();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel jupiter = SupplierReadModel.Create("Jupiter", "JPT");
            SupplierReadModel stoneMark = SupplierReadModel.Create("Stone Mark", "STM");
            IList<SupplierReadModel> suppliers = [jupiter, stoneMark];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = suppliers.Select(x => x.Id);

            // Act
            int result = await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel glassNGlass = SupplierReadModel.Create("Glass n Glass", "GNG");
            SupplierReadModel woodDeal = SupplierReadModel.Create("Wood Deal", "WDD");
            IList<SupplierReadModel> suppliers = [glassNGlass, woodDeal];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = suppliers.Select(x => x.GlobalUId);

            // Act
            Func<Task<int>> result = async () => await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<FunctionalityNotAvailableException>();
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Digital Star", "DS");
            await sut.Store<SupplierReadModel>(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<SupplierReadModel>(supplier.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<SupplierReadModel>(supplier.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Rockstone", "RST");
            await sut.Store<SupplierReadModel>(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<SupplierReadModel>(supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<SupplierReadModel>(supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel fastTrack = SupplierReadModel.Create("Fast Track", "FT");
            SupplierReadModel fancyWear = SupplierReadModel.Create("Fancy Wear", "FW");
            IList<SupplierReadModel> suppliers = [fastTrack, fancyWear];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = suppliers.Select(x => x.Id);
            await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToRestore = suppliers.Select(x => x.Id);

            // Act
            int result = await sut.BulkRestore<SupplierReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel skyTech = SupplierReadModel.Create("Sky Tech", "SKT");
            SupplierReadModel electroMagic = SupplierReadModel.Create("Electro Magic", "EM");
            IList<SupplierReadModel> suppliers = [skyTech, electroMagic];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = suppliers.Select(x => x.Id);
            await sut.BulkSoftDelete<SupplierReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = suppliers.Select(x => x.GlobalUId);

            // Act
            Func<Task<int>> result = async () => await sut.BulkRestore<SupplierReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<FunctionalityNotAvailableException>();
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Sweet Cleaner", "SC");
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            long result = await sut.Count<SupplierReadModel>(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            SupplierReadModel supplier = SupplierReadModel.Create("Digital Stereo", "DSR");
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            SupplierReadModel result = await sut.Find<SupplierReadModel>(supplier.Id, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

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
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            SupplierReadModel result = await sut.Find<SupplierReadModel>(supplier.GlobalUId, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

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
            IList<SupplierReadModel> suppliers = [crystalElectronics, crystalWoods];
            await sut.BulkStore<SupplierReadModel>(suppliers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            Expression<Func<SupplierReadModel, bool>> predicate = x => x.Name.Contains("Crystal") && x.EntityStatus == EntityStatus.Active;

            // Act
            IQueryable<SupplierReadModel> result = await sut.Filter(predicate, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

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
            IList<SupplierReadModel> suppliers = [smartAluminium, smartStone];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<SupplierReadModel> result = await sut.Get<SupplierReadModel>(0, 10, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

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
            IList<SupplierReadModel> suppliers = [microWood, microSteel, crownWood];
            await sut.BulkStore(suppliers.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<SupplierReadModel> result = await sut.Get<SupplierReadModel>(0, 10, predicate, false, RecordMode.Active, "Name", DataSortMode.Descending, TestContext.Current.CancellationToken).ConfigureAwait(false);

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

        [Fact]
        public async Task Supplier_validator_interceptor_executed()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            (string sampleSupplierName, string sampleSupplierCode) = SupplierReadModel.GetSampleSupplier();
            (string testSupplierName, string testSupplierCode) = SupplierReadModel.GetTestSupplier();
            SupplierReadModel supplier = SupplierReadModel.Create(sampleSupplierName, sampleSupplierCode);

            // Act
            await sut.Store(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);
            SupplierReadModel result = await sut.Find<SupplierReadModel>(supplier.GlobalUId, false, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be(testSupplierName);
                result.Code.Should().Be(testSupplierCode);
            }
        }
    }
}
