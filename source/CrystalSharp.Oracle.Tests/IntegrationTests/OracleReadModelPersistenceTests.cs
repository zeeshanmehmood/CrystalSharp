using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Oracle.ReadModels;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CrystalSharp.Oracle.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.OracleReadModelPersistenceIntegration)]
    public class OracleReadModelPersistenceTests(OracleReadModelStoreTestFixture fixture) : IClassFixture<OracleReadModelStoreTestFixture>
    {
        private readonly OracleReadModelStoreTestFixture _testFixture = fixture;

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Sylvester Webb", "SLW");

            // Act
            int result = await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Mark Anthony", "MRA");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Jack Wayne", "JWY");
            CustomerReadModel thirdCustomer = CustomerReadModel.Create("Dan Thomas", "DTM");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer, thirdCustomer];

            // Act
            int result = await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Trevor Anderson", "TAD");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);
            customer.Change("Ron Christopher", "RCS");

            // Act
            int result = await sut.Update(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("John Elliot", "JLI");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<CustomerReadModel>(customer.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Jason Kurt", "JSK");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<CustomerReadModel>(customer.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Ellen Bolt", "ELB");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<CustomerReadModel>(customer.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Nancy Fuller", "NCF");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<CustomerReadModel>(customer.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Fuller Davolio", "FLD");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Andrew Leverling", "AWL");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = customers.Select(x => x.Id);

            // Act
            int result = await sut.BulkDelete<CustomerReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Janet Nancy", "JTN");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Andrew Davolio", "ADL");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = customers.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkDelete<CustomerReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Janet Fuller", "JTF");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Janet Davolio", "JTD");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = customers.Select(x => x.Id);

            // Act
            int result = await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Nancy Leverling", "NCL");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Margaret Leverling", "MGL");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = customers.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Fuller Margaret", "FLM");
            await sut.Store<CustomerReadModel>(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<CustomerReadModel>(customer.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<CustomerReadModel>(customer.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Janet Andrew", "JTA");
            await sut.Store<CustomerReadModel>(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<CustomerReadModel>(customer.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<CustomerReadModel>(customer.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Margaret Davolio", "MGD");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Andrew Steven", "AES");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = customers.Select(x => x.Id);
            await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToRestore = customers.Select(x => x.Id);

            // Act
            int result = await sut.BulkRestore<CustomerReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Nancy Buchanan", "NCB");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Steven Leverling", "STL");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = customers.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = customers.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkRestore<CustomerReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Janet Buchanan", "JNB");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            long result = await sut.Count<CustomerReadModel>(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Steven Fuller", "STF");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            CustomerReadModel result = await sut.Find<CustomerReadModel>(customer.Id, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(customer.Id);
                result.GlobalUId.Should().Be(customer.GlobalUId);
                result.Name.Should().Be(customer.Name);
                result.Code.Should().Be(customer.Code);
            }
        }

        [Fact]
        public async Task Find_read_model_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Andrew Buchanan", "AWB");
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            CustomerReadModel result = await sut.Find<CustomerReadModel>(customer.GlobalUId, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(customer.Id);
                result.GlobalUId.Should().Be(customer.GlobalUId);
                result.Name.Should().Be(customer.Name);
                result.Code.Should().Be(customer.Code);
            }
        }

        [Fact]
        public async Task Filter_read_model_by_predicate()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("George Andrew", "GEA");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("George Dan", "GED");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore<CustomerReadModel>(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);
            Expression<Func<CustomerReadModel, bool>> predicate = x => x.Name.StartsWith("George") && x.EntityStatus == EntityStatus.Active;

            // Act
            IQueryable<CustomerReadModel> result = await sut.Filter(predicate, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Count().Should().Be(2);
                result.SingleOrDefault(x => x.Name == firstCustomer.Name).Should().NotBeNull();
                result.SingleOrDefault(x => x.Name == secondCustomer.Name).Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_all_records()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Nancy Suyama", "NCS");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Robert Fuller", "RBF");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<CustomerReadModel> result = await sut.Get<CustomerReadModel>(0, 10, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

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
            Expression<Func<CustomerReadModel, bool>> predicate = x => x.Name.ToLower().Contains("webb");
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Laura Webb", "LRW");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Steven Callahan", "SNC");
            CustomerReadModel thirdCustomer = CustomerReadModel.Create("John Webb", "JHW");
            IList<CustomerReadModel> customers = [firstCustomer, secondCustomer, thirdCustomer];
            await sut.BulkStore(customers, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<CustomerReadModel> result = await sut.Get<CustomerReadModel>(0, 10, predicate, false, RecordMode.Active, "Name", DataSortMode.Descending, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PageCount.Should().BeGreaterThan(0);
                result.PageSize.Should().BeGreaterThan(0);
                result.RowCount.Should().BeGreaterThan(0);
                result.Data.Count().Should().BeGreaterThan(0);
                result.Data.SingleOrDefault(x => x.GlobalUId == thirdCustomer.GlobalUId).Name.Should().Be(thirdCustomer.Name);
                result.Data.SingleOrDefault(x => x.GlobalUId == firstCustomer.GlobalUId).Name.Should().Be(firstCustomer.Name);
            }
        }

        [Fact]
        public async Task Customer_validator_interceptor_executed()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            (string sampleCustomerName, string sampleCustomerCode) = CustomerReadModel.GetSampleCustomer();
            (string testCustomerName, string testCustomerCode) = CustomerReadModel.GetTestCustomer();
            CustomerReadModel customer = CustomerReadModel.Create(sampleCustomerName, sampleCustomerCode);

            // Act
            await sut.Store(customer, TestContext.Current.CancellationToken).ConfigureAwait(false);
            CustomerReadModel result = await sut.Find<CustomerReadModel>(customer.GlobalUId, false, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be(testCustomerName);
                result.Code.Should().Be(testCustomerCode);
            }
        }
    }
}
