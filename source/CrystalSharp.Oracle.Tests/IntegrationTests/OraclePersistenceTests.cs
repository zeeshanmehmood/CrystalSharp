using CrystalSharp.Common.Utilities;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate;
using CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate;
using CrystalSharp.Tests.Common.Oracle.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CrystalSharp.Oracle.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.OracleIntegration)]
    public class OraclePersistenceTests(OracleTestFixture fixture) : IClassFixture<OracleTestFixture>
    {
        private readonly OracleTestFixture _testFixture = fixture;

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Sylvester Webb", "SLW");
            await sut.Employee.AddAsync(employee, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_and_code_are_equal()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Mark Anthony", "MRA");
            await sut.Employee.AddAsync(employee, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Mark Anthony");
                result.Code.Should().Be("MRA");
            }
        }

        [Fact]
        public async Task New_name_and_code_are_equal()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Jack Wayne", "JWY");
            await sut.Employee.AddAsync(employee, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            employee.Change("Dan Thomas", "DTM");
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Dan Thomas");
                result.Code.Should().Be("DTM");
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Trevor Anderson", "TAD");
            await sut.Employee.AddAsync(employee, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            employee.Delete();
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Ron Christopher", "RCS");
            await sut.Employee.AddAsync(employee, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Sale_order_and_order_details_saved()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            string saleOrderCode = $"SALE-ORDER-{RandomGenerator.GenerateNumber()}";
            SaleOrder saleOrder = SaleOrder.Create(saleOrderCode);
            saleOrder.AddOrderDetail("Headset", 2, 20.25M);
            saleOrder.AddOrderDetail("Mousepad", 5, 5);
            saleOrder.AddOrderDetail("Keyboard", 2, 73.52M);
            decimal amount = saleOrder.TotalAmount;
            saleOrder.Validate();
            await sut.SaleOrder.AddAsync(saleOrder, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            SaleOrder result = await sut.SaleOrder
                .Include(x => x.Orders)
                .SingleOrDefaultAsync(y => y.GlobalUId == saleOrder.GlobalUId, TestContext.Current.CancellationToken)
                .ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Code.Should().Be(saleOrderCode);
                result.Orders.Should().HaveCount(saleOrder.Orders.Count);
                result.TotalAmount.Should().Be(amount);
            }
        }

        [Fact]
        public async Task Employee_name_validator_interceptor_executed()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            string sampleEmployeeName = Employee.GetSampleEmployeeName();
            string testEmployeeName = Employee.GetTestEmployeeName();
            string employeeCode = "N/A";
            Employee employee = Employee.Create(sampleEmployeeName, employeeCode);
            await sut.Employee.AddAsync(employee, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Name.Should().Be(testEmployeeName);
        }

        [Fact]
        public async Task Sale_order_code_validator_interceptor_executed()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            string sampleSaleOrderCode = SaleOrder.GetSampleSaleOrderCode();
            string testSaleOrderCode = SaleOrder.GetTestSaleOrderCode();
            SaleOrder saleOrder = SaleOrder.Create(sampleSaleOrderCode);
            saleOrder.AddOrderDetail("Headset", 2, 20.25M);
            saleOrder.Validate();
            await sut.SaleOrder.AddAsync(saleOrder, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            SaleOrder result = await sut.SaleOrder
                .Include(x => x.Orders)
                .SingleOrDefaultAsync(y => y.GlobalUId == saleOrder.GlobalUId, TestContext.Current.CancellationToken)
                .ConfigureAwait(false);

            // Assert
            result.Code.Should().Be(testSaleOrderCode);
        }
    }
}
