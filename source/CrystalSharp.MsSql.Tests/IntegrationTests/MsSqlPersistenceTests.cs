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
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate;
using CrystalSharp.Tests.Common.MsSql.Infrastructure;

namespace CrystalSharp.MsSql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MsSqlIntegration)]
    public class MsSqlPersistenceTests : IClassFixture<MsSqlTestFixture>
    {
        private readonly MsSqlTestFixture _testFixture;

        public MsSqlPersistenceTests(MsSqlTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("United States dollar", new CurrencyInfo("USD", 840));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_and_codes_are_equal()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("Pound sterling", new CurrencyInfo("GBP", 826));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Pound sterling");
                result.CurrencyInfo.Code.Should().Be("GBP");
                result.CurrencyInfo.NumericCode.Should().Be(826);
            }
        }

        [Fact]
        public async Task New_name_and_codes_are_equal()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("Hungarian forint", new CurrencyInfo("HUF", 348));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            currency.ChangeName("Pakistani rupee");
            currency.ChangeInfo(new CurrencyInfo("PKR", 586));
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Pakistani rupee");
                result.CurrencyInfo.Code.Should().Be("PKR");
                result.CurrencyInfo.NumericCode.Should().Be(586);
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("Latvian lats", new CurrencyInfo("LVL", 111));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            currency.Delete();
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("Euro", new CurrencyInfo("EUR", 978));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
