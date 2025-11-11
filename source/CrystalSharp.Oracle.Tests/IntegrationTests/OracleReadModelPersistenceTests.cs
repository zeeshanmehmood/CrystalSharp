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
using CrystalSharp.Tests.Common.Oracle.ReadModels;

namespace CrystalSharp.Oracle.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.OracleReadModelPersistenceIntegration)]
    public class OracleReadModelPersistenceTests : IClassFixture<OracleReadModelStoreTestFixture>
    {
        private readonly OracleReadModelStoreTestFixture _testFixture;

        public OracleReadModelPersistenceTests(OracleReadModelStoreTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Sylvester Webb", "SLW");

            // Act
            bool result = await sut.Store(customer).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Mark Anthony", "MRA");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Jack Wayne", "JWY");
            CustomerReadModel thirdCustomer = CustomerReadModel.Create("Dan Thomas", "DTM");
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer, thirdCustomer };

            // Act
            bool result = await sut.BulkStore(customers).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Trevor Anderson", "TAD");
            await sut.Store(customer).ConfigureAwait(false);
            customer.Change("Ron Christopher", "RCS");

            // Act
            bool result = await sut.Update(customer).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("John Elliot", "JLI");
            await sut.Store(customer).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<CustomerReadModel>(customer.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Jason Kurt", "JSK");
            await sut.Store(customer).ConfigureAwait(false);

            // Act
            bool result = await sut.Delete<CustomerReadModel>(customer.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Ellen Bolt", "ELB");
            await sut.Store(customer).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<CustomerReadModel>(customer.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Nancy Fuller", "NCF");
            await sut.Store(customer).ConfigureAwait(false);

            // Act
            bool result = await sut.SoftDelete<CustomerReadModel>(customer.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Fuller Davolio", "FLD");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Andrew Leverling", "AWL");
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = customers.Select(x => x.Id);

            // Act
            bool result = await sut.BulkDelete<CustomerReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Janet Nancy", "JTN");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Andrew Davolio", "ADL");
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = customers.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkDelete<CustomerReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Janet Fuller", "JTF");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Janet Davolio", "JTD");
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = customers.Select(x => x.Id);

            // Act
            bool result = await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Nancy Leverling", "NCL");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Margaret Leverling", "MGL");
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = customers.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Fuller Margaret", "FLM");
            await sut.Store<CustomerReadModel>(customer).ConfigureAwait(false);
            await sut.SoftDelete<CustomerReadModel>(customer.Id).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<CustomerReadModel>(customer.Id).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Janet Andrew", "JTA");
            await sut.Store<CustomerReadModel>(customer).ConfigureAwait(false);
            await sut.SoftDelete<CustomerReadModel>(customer.GlobalUId).ConfigureAwait(false);

            // Act
            bool result = await sut.Restore<CustomerReadModel>(customer.GlobalUId).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Margaret Davolio", "MGD");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Andrew Steven", "AES");
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);
            IEnumerable<int> recordsToDelete = customers.Select(x => x.Id);
            await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<int> recordsToRestore = customers.Select(x => x.Id);

            // Act
            bool result = await sut.BulkRestore<CustomerReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel firstCustomer = CustomerReadModel.Create("Nancy Buchanan", "NCB");
            CustomerReadModel secondCustomer = CustomerReadModel.Create("Steven Leverling", "STL");
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = customers.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<CustomerReadModel>(recordsToDelete).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = customers.Select(x => x.GlobalUId);

            // Act
            bool result = await sut.BulkRestore<CustomerReadModel>(recordsToRestore).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Janet Buchanan", "JNB");
            await sut.Store(customer).ConfigureAwait(false);

            // Act
            long result = await sut.Count<CustomerReadModel>().ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<int> sut = _testFixture.ReadModelStore;
            CustomerReadModel customer = CustomerReadModel.Create("Steven Fuller", "STF");
            await sut.Store(customer).ConfigureAwait(false);

            // Act
            CustomerReadModel result = await sut.Find<CustomerReadModel>(customer.Id).ConfigureAwait(false);

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
            await sut.Store(customer).ConfigureAwait(false);

            // Act
            CustomerReadModel result = await sut.Find<CustomerReadModel>(customer.GlobalUId).ConfigureAwait(false);

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
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore<CustomerReadModel>(customers, CancellationToken.None).ConfigureAwait(false);
            Expression<Func<CustomerReadModel, bool>> predicate = x => x.Name.StartsWith("George") && x.EntityStatus == EntityStatus.Active;

            // Act
            IQueryable<CustomerReadModel> result = await sut.Filter(predicate).ConfigureAwait(false);

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
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);

            // Act
            PagedResult<CustomerReadModel> result = await sut.Get<CustomerReadModel>(0, 10).ConfigureAwait(false);

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
            IList<CustomerReadModel> customers = new List<CustomerReadModel> { firstCustomer, secondCustomer, thirdCustomer };
            await sut.BulkStore(customers).ConfigureAwait(false);

            // Act
            PagedResult<CustomerReadModel> result = await sut.Get<CustomerReadModel>(0, 10, predicate, false, RecordMode.Active, "Name", DataSortMode.Descending, CancellationToken.None).ConfigureAwait(false);

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
    }
}
