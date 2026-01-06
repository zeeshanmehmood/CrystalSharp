using CrystalSharp.Domain;
using CrystalSharp.MongoDb.Database;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MongoDb.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MongoDbIntegration)]
    public class MongoDbPersistenceTests(MongoDbTestFixture fixture) : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _testFixture = fixture;

        [Fact]
        public async Task Document_persisted()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonDetails("Jack", "Smith"), "jack.smith@test.com");

            // Act
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact result = await sut.Find<Contact>(contact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Firstname_lastname_and_email_are_equal()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonDetails("Terry", "Dan"), "terry.dan@test.com");

            // Act
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact result = await sut.Find<Contact>(contact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.PersonDetails.FirstName.Should().Be("Terry");
                result.PersonDetails.LastName.Should().Be("Dan");
                result.Email.Should().Be("terry.dan@test.com");
            }
        }

        [Fact]
        public async Task New_firstname_lastname_and_email_are_equal()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonDetails("Andy", "Williams"), "andy.williams@test.com");
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact existingContact = await sut.Find<Contact>(contact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Act
            existingContact.Change(new PersonDetails("Robert", "Wilson"), "robert.wilson@test.com");
            await sut.SaveChanges(existingContact, CancellationToken.None).ConfigureAwait(false);
            Contact result = await sut.Find<Contact>(existingContact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.PersonDetails.FirstName.Should().Be("Robert");
                result.PersonDetails.LastName.Should().Be("Wilson");
                result.Email.Should().Be("robert.wilson@test.com");
            }
        }

        [Fact]
        public async Task Document_is_deleted()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonDetails("John", "Martin"), "john.martin@test.com");
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact existingContact = await sut.Find<Contact>(contact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Act
            existingContact.Delete();
            await sut.SaveChanges(existingContact, CancellationToken.None);
            Contact result = await sut.Find<Contact>(existingContact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonDetails("Ted", "Thomson"), "ted.thomson@test.com");

            // Act
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact result = sut.Query<Contact>(x => x.Id == contact.Id).SingleOrDefault();

            // Assert
            result.Should().NotBeNull();
        }
    }
}
