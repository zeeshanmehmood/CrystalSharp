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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using CrystalSharp.Domain;
using CrystalSharp.MongoDb.Database;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate;

namespace CrystalSharp.MongoDb.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MongoDbIntegration)]
    public class MongoDbPersistenceTests : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _testFixture;

        public MongoDbPersistenceTests(MongoDbTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Document_persisted()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonInfo("Jack", "Smith"), "jack.smith@test.com");

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
            Contact contact = Contact.Create(new PersonInfo("Terry", "Dan"), "terry.dan@test.com");

            // Act
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact result = await sut.Find<Contact>(contact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.PersonInfo.FirstName.Should().Be("Terry");
                result.PersonInfo.LastName.Should().Be("Dan");
                result.Email.Should().Be("terry.dan@test.com");
            }
        }

        [Fact]
        public async Task New_firstname_lastname_and_email_are_equal()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonInfo("Andy", "Williams"), "andy.williams@test.com");
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact existingContact = await sut.Find<Contact>(contact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Act
            existingContact.Change(new PersonInfo("Robert", "Wilson"), "robert.wilson@test.com");
            await sut.SaveChanges(existingContact, CancellationToken.None).ConfigureAwait(false);
            Contact result = await sut.Find<Contact>(existingContact.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.PersonInfo.FirstName.Should().Be("Robert");
                result.PersonInfo.LastName.Should().Be("Wilson");
                result.Email.Should().Be("robert.wilson@test.com");
            }
        }

        [Fact]
        public async Task Document_is_deleted()
        {
            // Arrange
            IMongoDbContext sut = _testFixture.MongoDbContext;
            Contact contact = Contact.Create(new PersonInfo("John", "Martin"), "john.martin@test.com");
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
            Contact contact = Contact.Create(new PersonInfo("Ted", "Thomson"), "ted.thomson@test.com");

            // Act
            await sut.SaveChanges(contact, CancellationToken.None).ConfigureAwait(false);
            Contact result = sut.Query<Contact>(x => x.Id == contact.Id).SingleOrDefault();

            // Assert
            result.Should().NotBeNull();
        }
    }
}
