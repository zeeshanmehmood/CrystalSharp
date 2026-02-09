using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.PostgreSql.ReadModels;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CrystalSharp.PostgreSql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.PostgreSqlReadModelPersistenceIntegration)]
    public class PostgreSqlReadModelPersistenceTests(PostgreSqlReadModelStoreTestFixture fixture) : IClassFixture<PostgreSqlReadModelStoreTestFixture>
    {
        private readonly PostgreSqlReadModelStoreTestFixture _testFixture = fixture;

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Information Technology", "IT");

            // Act
            int result = await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel softwareDevelopment = DepartmentReadModel.Create("Software Development", "SD");
            DepartmentReadModel qualityControl = DepartmentReadModel.Create("Quality Control", "QC");
            DepartmentReadModel qualityAssurance = DepartmentReadModel.Create("Quality Assurance", "QA");
            IList<DepartmentReadModel> departments = [softwareDevelopment, qualityControl, qualityAssurance];

            // Act
            int result = await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Hardware", "HW");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);
            department.Change("Finance", "FN");

            // Act
            int result = await sut.Update(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Management", "MGN");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<DepartmentReadModel>(department.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Store Management", "SM");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<DepartmentReadModel>(department.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Record Management", "RM");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<DepartmentReadModel>(department.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Dispute Management", "DM");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<DepartmentReadModel>(department.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel foodManagement = DepartmentReadModel.Create("Food Management", "FM");
            DepartmentReadModel wasteManagement = DepartmentReadModel.Create("Waste Management", "WM");
            IList<DepartmentReadModel> departments = [foodManagement, wasteManagement];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = departments.Select(x => x.Id);

            // Act
            int result = await sut.BulkDelete<DepartmentReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel generalManagement = DepartmentReadModel.Create("General Management", "GM");
            DepartmentReadModel advertisingManagement = DepartmentReadModel.Create("Advertising Management", "AD");
            IList<DepartmentReadModel> departments = [generalManagement, advertisingManagement];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = departments.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkDelete<DepartmentReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel salesManagement = DepartmentReadModel.Create("Sales Management", "SLM");
            DepartmentReadModel purchaseManagement = DepartmentReadModel.Create("Purchase Management", "PCM");
            IList<DepartmentReadModel> departments = [salesManagement, purchaseManagement];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = departments.Select(x => x.Id);

            // Act
            int result = await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel transportManagement = DepartmentReadModel.Create("Transport Management", "TM");
            DepartmentReadModel canteenManagement = DepartmentReadModel.Create("Canteen Management", "CM");
            IList<DepartmentReadModel> departments = [transportManagement, canteenManagement];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = departments.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Production Management", "PDM");
            await sut.Store<DepartmentReadModel>(department, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<DepartmentReadModel>(department.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<DepartmentReadModel>(department.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Marketing Management", "MM");
            await sut.Store<DepartmentReadModel>(department, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<DepartmentReadModel>(department.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<DepartmentReadModel>(department.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel inspectionManagement = DepartmentReadModel.Create("Inspection Management", "IM");
            DepartmentReadModel administrationManagement = DepartmentReadModel.Create("Administration Management", "AM");
            IList<DepartmentReadModel> departments = [inspectionManagement, administrationManagement];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = departments.Select(x => x.Id);
            await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<int> recordsToRestore = departments.Select(x => x.Id);

            // Act
            int result = await sut.BulkRestore<DepartmentReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel logisticsManagement = DepartmentReadModel.Create("Logistics Management", "LM");
            DepartmentReadModel assetManagement = DepartmentReadModel.Create("Asset Management", "ASM");
            IList<DepartmentReadModel> departments = [logisticsManagement, assetManagement];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = departments.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = departments.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkRestore<DepartmentReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Operations Management", "OM");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            long result = await sut.Count<DepartmentReadModel>(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Risk Management", "RSM");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            DepartmentReadModel result = await sut.Find<DepartmentReadModel>(department.Id, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(department.Id);
                result.GlobalUId.Should().Be(department.GlobalUId);
                result.Name.Should().Be(department.Name);
                result.Code.Should().Be(department.Code);
            }
        }

        [Fact]
        public async Task Find_read_model_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Engineering Management", "EM");
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            DepartmentReadModel result = await sut.Find<DepartmentReadModel>(department.GlobalUId, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(department.Id);
                result.GlobalUId.Should().Be(department.GlobalUId);
                result.Name.Should().Be(department.Name);
                result.Code.Should().Be(department.Code);
            }
        }

        [Fact]
        public async Task Filter_read_model_by_predicate()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel salesAudit = DepartmentReadModel.Create("Audit - Sales", "AUDIT-SALES");
            DepartmentReadModel financeAudit = DepartmentReadModel.Create("Audit - Finance", "AUDIT-FINANCE");
            IList<DepartmentReadModel> departments = [salesAudit, financeAudit];
            await sut.BulkStore<DepartmentReadModel>(departments, TestContext.Current.CancellationToken).ConfigureAwait(false);
            Expression<Func<DepartmentReadModel, bool>> predicate = x => x.Name.StartsWith("Audit") && x.EntityStatus == EntityStatus.Active;

            // Act
            IQueryable<DepartmentReadModel> result = await sut.Filter(predicate, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Count().Should().Be(2);
                result.SingleOrDefault(x => x.Name == salesAudit.Name).Should().NotBeNull();
                result.SingleOrDefault(x => x.Name == financeAudit.Name).Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_all_records()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel customerSupport = DepartmentReadModel.Create("Customer Support", "CS");
            DepartmentReadModel investorRelations = DepartmentReadModel.Create("Investor Relations", "IVR");
            IList<DepartmentReadModel> departments = [customerSupport, investorRelations];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<DepartmentReadModel> result = await sut.Get<DepartmentReadModel>(0, 10, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

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
            Expression<Func<DepartmentReadModel, bool>> predicate = x => x.Name.ToLower().Contains("customer");
            DepartmentReadModel customerServiceManagement = DepartmentReadModel.Create("Customer Service Management", "CSM");
            DepartmentReadModel humanResourceManagement = DepartmentReadModel.Create("Human Resource Management", "HRM");
            DepartmentReadModel customerExperienceManagement = DepartmentReadModel.Create("Customer Experience Management", "CEM");
            IList<DepartmentReadModel> departments = [customerServiceManagement, humanResourceManagement, customerExperienceManagement];
            await sut.BulkStore(departments.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<DepartmentReadModel> result = await sut.Get<DepartmentReadModel>(0, 10, predicate, false, RecordMode.Active, "Name", DataSortMode.Descending, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PageCount.Should().BeGreaterThan(0);
                result.PageSize.Should().BeGreaterThan(0);
                result.RowCount.Should().BeGreaterThan(0);
                result.Data.Count().Should().BeGreaterThan(0);
                result.Data.SingleOrDefault(x => x.GlobalUId == customerServiceManagement.GlobalUId).Name.Should().Be(customerServiceManagement.Name);
                result.Data.SingleOrDefault(x => x.GlobalUId == customerExperienceManagement.GlobalUId).Name.Should().Be(customerExperienceManagement.Name);
            }
        }

        [Fact]
        public async Task Department_validator_interceptor_executed()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            (string sampleDepartmentName, string sampleDepartmentCode) = DepartmentReadModel.GetSampleDepartment();
            (string testDepartmentName, string testDepartmentCode) = DepartmentReadModel.GetTestDepartment();
            DepartmentReadModel department = DepartmentReadModel.Create(sampleDepartmentName, sampleDepartmentCode);

            // Act
            await sut.Store(department, TestContext.Current.CancellationToken).ConfigureAwait(false);
            DepartmentReadModel result = await sut.Find<DepartmentReadModel>(department.GlobalUId, false, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be(testDepartmentName);
                result.Code.Should().Be(testDepartmentCode);
            }
        }
    }
}
