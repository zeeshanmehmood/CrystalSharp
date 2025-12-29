using CrystalSharp.Common.Utilities;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate;
using CrystalSharp.Tests.Common.MsSql.Aggregates.InvoiceAggregate;
using CrystalSharp.Tests.Common.MsSql.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MsSql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MsSqlIntegration)]
    public class MsSqlPersistenceTests(MsSqlTestFixture fixture) : IClassFixture<MsSqlTestFixture>
    {
        private readonly MsSqlTestFixture _testFixture = fixture;

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("United States dollar", new CurrencyDetails("USD", 840));
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
            Currency currency = Currency.Create("Pound sterling", new CurrencyDetails("GBP", 826));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Pound sterling");
                result.CurrencyDetails.Code.Should().Be("GBP");
                result.CurrencyDetails.NumericCode.Should().Be(826);
            }
        }

        [Fact]
        public async Task New_name_and_codes_are_equal()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("Hungarian forint", new CurrencyDetails("HUF", 348));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            currency.ChangeName("Pakistani rupee");
            currency.ChangeDetails(new CurrencyDetails("PKR", 586));
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Pakistani rupee");
                result.CurrencyDetails.Code.Should().Be("PKR");
                result.CurrencyDetails.NumericCode.Should().Be(586);
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            Currency currency = Currency.Create("Latvian lats", new CurrencyDetails("LVL", 111));
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
            Currency currency = Currency.Create("Euro", new CurrencyDetails("EUR", 978));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Invoice_and_line_items_saved()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            string invoiceCode = $"INV-{RandomGenerator.GenerateNumber()}";
            Invoice invoice = Invoice.Create(invoiceCode);
            invoice.AddLineItem("Headset", 2, 20.25M);
            invoice.AddLineItem("Mousepad", 5, 5);
            invoice.AddLineItem("Keyboard", 2, 73.52M);
            decimal amount = invoice.TotalAmount;
            invoice.Validate();
            await sut.Invoice.AddAsync(invoice, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Invoice result = await sut.Invoice
                .Include(x => x.LineItems)
                .SingleOrDefaultAsync(y => y.GlobalUId == invoice.GlobalUId, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Code.Should().Be(invoiceCode);
                result.LineItems.Should().HaveCount(invoice.LineItems.Count);
                result.TotalAmount.Should().Be(amount);
            }
        }

        [Fact]
        public async Task Currency_name_validator_interceptor_executed()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            string sampleCurrencyName = Currency.GetSampleCurrencyName();
            string testCurrencyName = Currency.GetTestCurrencyName();
            string currencyCode = "N/A";
            int currencyNumericCode = 1;
            Currency currency = Currency.Create(sampleCurrencyName, new CurrencyDetails(currencyCode, currencyNumericCode));
            await sut.Currency.AddAsync(currency, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Currency result = await sut.Currency.SingleOrDefaultAsync(x => x.GlobalUId == currency.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Name.Should().Be(testCurrencyName);
        }

        [Fact]
        public async Task Invoice_code_validator_interceptor_executed()
        {
            // Arrange
            IMsSqlDataContext sut = _testFixture.DataContext;
            string sampleInvoiceCode = Invoice.GetSampleInvoiceCode();
            string testInvoiceCode = Invoice.GetTestInvoiceCode();
            Invoice invoice = Invoice.Create(sampleInvoiceCode);
            invoice.AddLineItem("Headset", 5, 20.25M);
            invoice.Validate();
            await sut.Invoice.AddAsync(invoice, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Invoice result = await sut.Invoice
                .Include(x => x.LineItems)
                .SingleOrDefaultAsync(y => y.GlobalUId == invoice.GlobalUId, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Code.Should().Be(testInvoiceCode);
        }
    }
}
