using CrystalSharp.Common.Utilities;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MongoDb.ReadModels;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CrystalSharp.MongoDb.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MongoDbReadModelPersistenceIntegration)]
    public class MongoDbReadModelPersistenceTests(MongoDbTestFixture fixture) : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _testFixture = fixture;

        [Fact]
        public async Task Read_model_persisted()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Nancy", "Fuller", "nancy.fuller@test.com");

            // Act
            int result = await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_persisted()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Fuller", "Davolio", "fuller.davolio@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Andrew", "Leverling", "andrew.leverling@test.com");
            ContactReadModel thirdContact = ContactReadModel.Create("Janet", "Nancy", "janet.nancy@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact, thirdContact];

            // Act
            int result = await sut.BulkStore(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_updated()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Andrew", "Davolio", "andrew.davolio@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);
            contact.Change("Janet", "Fuller", "janet.fuller@test.com");

            // Act
            int result = await sut.Update(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Janet", "Davolio", "janet.davolio@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<ContactReadModel>(contact.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Nancy", "Leverling", "nancy.leverling@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Delete<ContactReadModel>(contact.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Margaret", "Leverling", "margaret.leverling@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<ContactReadModel>(contact.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Fuller", "Margaret", "fuller.margaret@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.SoftDelete<ContactReadModel>(contact.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Janet", "Andrew", "janet.andrew@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Margaret", "Davolio", "margaret.davolio@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<string> recordsToDelete = contacts.Select(x => x.Id);

            // Act
            int result = await sut.BulkDelete<ContactReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Andrew", "Steven", "andrew.steven@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Nancy", "Buchanan", "nancy.buchanan@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = contacts.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkDelete<ContactReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Steven", "Leverling", "steven.leverling@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Janet", "Buchanan", "janet.buchanan@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<string> recordsToDelete = contacts.Select(x => x.Id);

            // Act
            int result = await sut.BulkSoftDelete<ContactReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_soft_deleted_by_globaluid()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Steven", "Fuller", "steven.fuller@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Andrew", "Buchanan", "andrew.buchanan@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = contacts.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkSoftDelete<ContactReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_restored()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Nancy", "Suyama", "nancy.suyama@test.com");
            await sut.Store<ContactReadModel>(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<ContactReadModel>(contact.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<ContactReadModel>(contact.Id, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Robert", "Fuller", "robert.fuller@test.com");
            await sut.Store<ContactReadModel>(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SoftDelete<ContactReadModel>(contact.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            int result = await sut.Restore<ContactReadModel>(contact.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Andrew", "Robert", "andrew.robert@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Janet", "Suyama", "janet.suyama@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<string> recordsToDelete = contacts.Select(x => x.Id);
            await sut.BulkSoftDelete<ContactReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<string> recordsToRestore = contacts.Select(x => x.Id);

            // Act
            int result = await sut.BulkRestore<ContactReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Bulk_read_model_restored_by_globaluid()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Nancy", "Dodsworth", "nancy.dodsworth@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Laura", "Leverling", "laura.leverling@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToDelete = contacts.Select(x => x.GlobalUId);
            await sut.BulkSoftDelete<ContactReadModel>(recordsToDelete, TestContext.Current.CancellationToken).ConfigureAwait(false);
            IEnumerable<Guid> recordsToRestore = contacts.Select(x => x.GlobalUId);

            // Act
            int result = await sut.BulkRestore<ContactReadModel>(recordsToRestore, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Count_read_model()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Suyama", "Davolio", "suyama.davolio@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            long result = await sut.Count<ContactReadModel>(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Find_read_model_by_id()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Robert", "Leverling", "robert.leverling@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            ContactReadModel result = await sut.Find<ContactReadModel>(contact.Id, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(contact.Id);
                result.GlobalUId.Should().Be(contact.GlobalUId);
                result.FirstName.Should().Be(contact.FirstName);
                result.LastName.Should().Be(contact.LastName);
                result.Email.Should().Be(contact.Email);
            }
        }

        [Fact]
        public async Task Find_read_model_by_globaluid()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel contact = ContactReadModel.Create("Margaret", "Suyama", "margaret.suyama@test.com");
            await sut.Store(contact, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            ContactReadModel result = await sut.Find<ContactReadModel>(contact.GlobalUId, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be(contact.Id);
                result.GlobalUId.Should().Be(contact.GlobalUId);
                result.FirstName.Should().Be(contact.FirstName);
                result.LastName.Should().Be(contact.LastName);
                result.Email.Should().Be(contact.Email);
            }
        }

        [Fact]
        public async Task Filter_read_model_by_predicate()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            string randomEmailKey = $"{RandomGenerator.GenerateString()}{RandomGenerator.GenerateNumber()}";
            ContactReadModel firstContact = ContactReadModel.Create("George", "Andrew", $"george.andrew.{randomEmailKey}@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("George", "Dan", $"george.dan.{randomEmailKey}@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore(contacts, TestContext.Current.CancellationToken).ConfigureAwait(false);
            Expression<Func<ContactReadModel, bool>> predicate = x => x.Email.Contains(randomEmailKey) && x.EntityStatus == EntityStatus.Active;

            // Act
            IQueryable<ContactReadModel> result = await sut.Filter(predicate, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Count().Should().Be(2);
                result.SingleOrDefault(x => x.Email == firstContact.Email).Should().NotBeNull();
                result.SingleOrDefault(x => x.Email == secondContact.Email).Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_all_records()
        {
            // Arrange
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            ContactReadModel firstContact = ContactReadModel.Create("Andrew", "Callahan", "andrew.callahan@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Robert", "Buchanan", "robert.buchanan@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact];
            await sut.BulkStore<ContactReadModel>(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<ContactReadModel> result = await sut.Get<ContactReadModel>(0, 10, cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);

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
            IReadModelStore<string> sut = _testFixture.ReadModelStore;
            Expression<Func<ContactReadModel, bool>> predicate = x => x.LastName.ToLower().Contains("web");
            ContactReadModel firstContact = ContactReadModel.Create("Laura", "Webb", "laura.webb@test.com");
            ContactReadModel secondContact = ContactReadModel.Create("Steven", "Callahan", "steven.callahan@test.com");
            ContactReadModel thirdContact = ContactReadModel.Create("John", "Webbs", "john.webbs@test.com");
            IList<ContactReadModel> contacts = [firstContact, secondContact, thirdContact];
            await sut.BulkStore<ContactReadModel>(contacts.AsEnumerable(), TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            PagedResult<ContactReadModel> result = await sut.Get<ContactReadModel>(
                0,
                10,
                predicate,
                false,
                RecordMode.Active,
                "FirstName",
                DataSortMode.Ascending,
                TestContext.Current.CancellationToken)
                .ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PageCount.Should().BeGreaterThan(0);
                result.PageSize.Should().BeGreaterThan(0);
                result.RowCount.Should().BeGreaterThan(0);
                result.Data.Count().Should().BeGreaterThan(0);
                result.Data.SingleOrDefault(x => x.GlobalUId == firstContact.GlobalUId).FirstName.Should().Be(firstContact.FirstName);
                result.Data.SingleOrDefault(x => x.GlobalUId == thirdContact.GlobalUId).FirstName.Should().Be(thirdContact.FirstName);
            }
        }
    }
}
