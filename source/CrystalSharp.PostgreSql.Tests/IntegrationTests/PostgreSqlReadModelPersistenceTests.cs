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
using CrystalSharp.Tests.Common.PostgreSql.ReadModels;

namespace CrystalSharp.PostgreSql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.PostgreSqlReadModelPersistenceIntegration)]
    public class PostgreSqlReadModelPersistenceTests : IClassFixture<PostgreSqlReadModelStoreTestFixture>
    {
        private readonly PostgreSqlReadModelStoreTestFixture _testFixture;

        public PostgreSqlReadModelPersistenceTests(PostgreSqlReadModelStoreTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Information Technology", "IT");

            // Act
            bool result = await sut.Store(department).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel softwareDevelopment = DepartmentReadModel.Create("Software Development", "SD");
            DepartmentReadModel qualityControl = DepartmentReadModel.Create("Quality Control", "QC");
            DepartmentReadModel qualityAssurance = DepartmentReadModel.Create("Quality Assurance", "QA");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { softwareDevelopment, qualityControl, qualityAssurance };

            // Act
            bool result = await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Hardware", "HW");
            await sut.Store(department).ConfigureAwait(false);
            department.Change("Finance", "FN");

            // Act
            bool result = await sut.Update(department).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Management", "MGN");
            await sut.Store(department).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<DepartmentReadModel>(department.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Store Management", "SM");
            await sut.Store(department).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<DepartmentReadModel>(department.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Record Management", "RM");
            await sut.Store(department).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<DepartmentReadModel>(department.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Dispute Management", "DM");
            await sut.Store(department).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<DepartmentReadModel>(department.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel foodManagement = DepartmentReadModel.Create("Food Management", "FM");
            DepartmentReadModel wasteManagement = DepartmentReadModel.Create("Waste Management", "WM");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { foodManagement, wasteManagement };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = departments.Select(x => x.Id);

            // Act
            bool result = await sut.BulkDelete<DepartmentReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel generalManagement = DepartmentReadModel.Create("General Management", "GM");
            DepartmentReadModel advertisingManagement = DepartmentReadModel.Create("Advertising Management", "AD");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { generalManagement, advertisingManagement };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = departments.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkDelete<DepartmentReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel salesManagement = DepartmentReadModel.Create("Sales Management", "SLM");
            DepartmentReadModel purchaseManagement = DepartmentReadModel.Create("Purchase Management", "PCM");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { salesManagement, purchaseManagement };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = departments.Select(x => x.Id);

            // Act
            bool result = await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel transportManagement = DepartmentReadModel.Create("Transport Management", "TM");
            DepartmentReadModel canteenManagement = DepartmentReadModel.Create("Canteen Management", "CM");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { transportManagement, canteenManagement };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = departments.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Production Management", "PDM");
            await sut.Store<DepartmentReadModel>(department).ConfigureAwait(false);
            await sut.SoftDelete<DepartmentReadModel>(department.Id).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<DepartmentReadModel>(department.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Marketing Management", "MM");
            await sut.Store<DepartmentReadModel>(department).ConfigureAwait(false);
            await sut.SoftDelete<DepartmentReadModel>(department.GlobalUId).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<DepartmentReadModel>(department.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel inspectionManagement = DepartmentReadModel.Create("Inspection Management", "IM");
            DepartmentReadModel administrationManagement = DepartmentReadModel.Create("Administration Management", "AM");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { inspectionManagement, administrationManagement };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = departments.Select(x => x.Id);
            await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<int> recordsToRestore = departments.Select(x => x.Id);

            // Act
            bool result = await sut.BulkRestore<DepartmentReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel logisticsManagement = DepartmentReadModel.Create("Logistics Management", "LM");
            DepartmentReadModel assetManagement = DepartmentReadModel.Create("Asset Management", "ASM");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { logisticsManagement, assetManagement };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = departments.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<DepartmentReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = departments.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkRestore<DepartmentReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Operations Management", "OM");
            await sut.Store(department).ConfigureAwait(false);

            // Act
            long result = await sut.Count<DepartmentReadModel>().ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel department = DepartmentReadModel.Create("Risk Management", "RSM");
            await sut.Store(department).ConfigureAwait(false);

            // Act
            DepartmentReadModel result = await sut.Find<DepartmentReadModel>(department.Id).ConfigureAwait(false);

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
            await sut.Store(department).ConfigureAwait(false);

            // Act
            DepartmentReadModel result = await sut.Find<DepartmentReadModel>(department.GlobalUId).ConfigureAwait(false);

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
        public async Task Get_all_records()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            DepartmentReadModel customerSupport = DepartmentReadModel.Create("Customer Support", "CS");
            DepartmentReadModel investorRelations = DepartmentReadModel.Create("Investor Relations", "IVR");
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { customerSupport, investorRelations };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);

            // Act
            PagedResult<DepartmentReadModel> result = await sut.Get<DepartmentReadModel>(0, 10).ConfigureAwait(false);

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
            IList<DepartmentReadModel> departments = new List<DepartmentReadModel> { customerServiceManagement, humanResourceManagement, customerExperienceManagement };
            await sut.BulkStore(departments.AsEnumerable()).ConfigureAwait(false);

            // Act
            PagedResult<DepartmentReadModel> result = await sut.Get<DepartmentReadModel>(0, 10, predicate, RecordMode.Active, "Name", DataSortMode.Descending, CancellationToken.None).ConfigureAwait(false);

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
    }
}
