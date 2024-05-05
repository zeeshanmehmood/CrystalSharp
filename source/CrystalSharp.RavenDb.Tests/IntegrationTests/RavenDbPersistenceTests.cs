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
using Raven.TestDriver;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using CrystalSharp.Domain;
using CrystalSharp.RavenDb.Database;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.RavenDb.Aggregates.CountryAggregate;

namespace CrystalSharp.RavenDb.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.RavenDbIntegration)]
    public class RavenDbPersistenceTests : RavenTestDriver, IClassFixture<RavenDbTestFixture>
    {
        private readonly RavenDbTestFixture _testFixture;

        public RavenDbPersistenceTests(RavenDbTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Document_persisted()
        {
            // Arrange
            IRavenDbContext context = _testFixture.RavenDbContext;
            Country country = Country.Create("United States of America", new CountryInfo("US", 840));

            // Act
            context.Session.Store(country);
            await context.SaveChanges(CancellationToken.None).ConfigureAwait(false);
            WaitForIndexing(context.DocumentStore);
            Country result = context.Session.Query<Country>().Where(x => x.GlobalUId == country.GlobalUId).SingleOrDefault();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_and_codes_are_equal()
        {
            // Arrange
            IRavenDbContext context = _testFixture.RavenDbContext;
            Country country = Country.Create("United Kingdom", new CountryInfo("UK", 826));

            // Act
            context.Session.Store(country);
            await context.SaveChanges(CancellationToken.None).ConfigureAwait(false);
            WaitForIndexing(context.DocumentStore);
            Country result = context.Session.Query<Country>().Where(x => x.GlobalUId == country.GlobalUId).SingleOrDefault();

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("United Kingdom");
                result.CountryInfo.Code.Should().Be("UK");
                result.CountryInfo.NumericCode.Should().Be(826);
            }
        }

        [Fact]
        public async Task New_name_and_codes_are_equal()
        {
            // Arrange
            IRavenDbContext context = _testFixture.RavenDbContext;
            Country country = Country.Create("Hungary", new CountryInfo("HU", 348));
            context.Session.Store(country);
            await context.SaveChanges(CancellationToken.None).ConfigureAwait(false);
            WaitForIndexing(context.DocumentStore);
            country = context.Session.Load<Country>(country.Id);

            // Act
            country.ChangeName("Pakistan");
            country.ChangeInfo(new CountryInfo("PK", 586));
            await context.SaveChanges(CancellationToken.None).ConfigureAwait(false);
            WaitForIndexing(context.DocumentStore);
            Country result = context.Session.Query<Country>().Where(x => x.GlobalUId == country.GlobalUId).SingleOrDefault();

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Pakistan");
                result.CountryInfo.Code.Should().Be("PK");
                result.CountryInfo.NumericCode.Should().Be(586);
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IRavenDbContext context = _testFixture.RavenDbContext;
            Country country = Country.Create("Australia", new CountryInfo("AU", 036));
            context.Session.Store(country);
            await context.SaveChanges(CancellationToken.None).ConfigureAwait(false);
            WaitForIndexing(context.DocumentStore);
            country = context.Session.Load<Country>(country.Id);

            // Act
            country.Delete();
            await context.SaveChanges(CancellationToken.None).ConfigureAwait(false);
            WaitForIndexing(context.DocumentStore);
            Country result = context.Session.Query<Country>().Where(x => x.GlobalUId == country.GlobalUId).SingleOrDefault();

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IRavenDbContext context = _testFixture.RavenDbContext;
            Country country = Country.Create("Switzerland", new CountryInfo("CH", 756));
            context.Session.Store(country);
            await context.SaveChanges(CancellationToken.None).ConfigureAwait(false);
            WaitForIndexing(context.DocumentStore);

            // Act
            IQueryable<Country> result = context.Session.Query<Country>().Where(x => x.GlobalUId == country.GlobalUId);

            // Assert
            result.Should().NotBeNull().And.HaveCount(1);
        }
    }
}
