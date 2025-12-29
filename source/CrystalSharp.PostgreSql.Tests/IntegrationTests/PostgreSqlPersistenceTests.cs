using CrystalSharp.Common.Utilities;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.ReceiptAggregate;
using CrystalSharp.Tests.Common.PostgreSql.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.PostgreSql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.PostgreSqlIntegration)]
    public class PostgreSqlPersistenceTests(PostgreSqlTestFixture fixture) : IClassFixture<PostgreSqlTestFixture>
    {
        private readonly PostgreSqlTestFixture _testFixture = fixture;

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Information Technology", new DepartmentDetails("IT", "it.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_code_and_email_are_equal()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Software Development", new DepartmentDetails("SD", "software.development.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Software Development");
                result.DepartmentDetails.Code.Should().Be("SD");
                result.DepartmentDetails.Email.Should().Be("software.development.department@test.com");
            }
        }

        [Fact]
        public async Task New_name_code_and_email_are_equal()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Quality Control", new DepartmentDetails("QC", "quality.control.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            department.ChangeName("Quality Assurance");
            department.ChangeDetails(new DepartmentDetails("QA", "qa.department@test.com"));
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Quality Assurance");
                result.DepartmentDetails.Code.Should().Be("QA");
                result.DepartmentDetails.Email.Should().Be("qa.department@test.com");
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Hardware", new DepartmentDetails("HW", "hardware.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            department.Delete();
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Finance", new DepartmentDetails("FN", "finance.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Receipt_and_inventory_items_saved()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            string receiptCode = $"RECEIPT-{RandomGenerator.GenerateNumber()}";
            Receipt receipt = Receipt.Create(receiptCode);
            receipt.AddInventoryItem("Headset", 2, 20.25M);
            receipt.AddInventoryItem("Mousepad", 5, 5);
            receipt.AddInventoryItem("Keyboard", 2, 73.52M);
            decimal amount = receipt.TotalAmount;
            receipt.Validate();
            await sut.Receipt.AddAsync(receipt, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Receipt result = await sut.Receipt
                .Include(x => x.InventoryItems)
                .SingleOrDefaultAsync(y => y.GlobalUId == receipt.GlobalUId, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Code.Should().Be(receiptCode);
                result.InventoryItems.Should().HaveCount(receipt.InventoryItems.Count);
                result.TotalAmount.Should().Be(amount);
            }
        }

        [Fact]
        public async Task Department_name_validator_interceptor_executed()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            string sampleDepartmentName = Department.GetSampleDepartmentName();
            string testDepartmentName = Department.GetTestDepartmentName();
            string departmentCode = "N/A";
            string departmentEmail = "sample.test@test.com";
            Department department = Department.Create(sampleDepartmentName, new DepartmentDetails(departmentCode, departmentEmail));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Name.Should().Be(testDepartmentName);
        }

        [Fact]
        public async Task Receipt_code_validator_interceptor_executed()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            string sampleReceiptCode = Receipt.GetSampleReceiptCode();
            string testReceiptCode = Receipt.GetTestReceiptCode();
            Receipt receipt = Receipt.Create(sampleReceiptCode);
            receipt.AddInventoryItem("Headset", 2, 20.25M);
            receipt.Validate();
            await sut.Receipt.AddAsync(receipt, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Receipt result = await sut.Receipt.SingleOrDefaultAsync(x => x.GlobalUId == receipt.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Code.Should().Be(testReceiptCode);
        }
    }
}
