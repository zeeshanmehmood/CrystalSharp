using CrystalSharp.Common.Utilities;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate;
using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate;
using CrystalSharp.Tests.Common.MySql.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CrystalSharp.MySql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MySqlIntegration)]
    public class MySqlPersistenceTests(MySqlTestFixture fixture) : IClassFixture<MySqlTestFixture>
    {
        private readonly MySqlTestFixture _testFixture = fixture;

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Falcon", new SupplierDetails("FLC", "falcon@test.com"));
            await sut.Supplier.AddAsync(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_code_and_email_are_equal()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Cosmos", new SupplierDetails("CMS", "cosmos@test.com"));
            await sut.Supplier.AddAsync(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Cosmos");
                result.SupplierDetails.Code.Should().Be("CMS");
                result.SupplierDetails.Email.Should().Be("cosmos@test.com");
            }
        }

        [Fact]
        public async Task New_name_code_and_email_are_equal()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Three Star", new SupplierDetails("TSR", "three.star@test.com"));
            await sut.Supplier.AddAsync(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            supplier.ChangeName("Eagle");
            supplier.ChangeDetails(new SupplierDetails("EGL", "eagle@test.com"));
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Eagle");
                result.SupplierDetails.Code.Should().Be("EGL");
                result.SupplierDetails.Email.Should().Be("eagle@test.com");
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Sunshine", new SupplierDetails("SSH", "sunshine@test.com"));
            await sut.Supplier.AddAsync(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            supplier.Delete();
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("QQ North", new SupplierDetails("QQN", "qq.north@test.com"));
            await sut.Supplier.AddAsync(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Purchase_order_and_order_items_saved()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            string purchaseOrderCode = $"ORDER-{RandomGenerator.GenerateNumber()}";
            PurchaseOrder purchaseOrder = PurchaseOrder.Create(purchaseOrderCode);
            purchaseOrder.AddOrderItem("Headset", 2, 20.25M);
            purchaseOrder.AddOrderItem("Mousepad", 5, 5);
            purchaseOrder.AddOrderItem("Keyboard", 2, 73.52M);
            decimal amount = purchaseOrder.TotalAmount;
            purchaseOrder.Validate();
            await sut.PurchaseOrder.AddAsync(purchaseOrder, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            PurchaseOrder result = await sut.PurchaseOrder
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(y => y.GlobalUId == purchaseOrder.GlobalUId, TestContext.Current.CancellationToken)
                .ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Code.Should().Be(purchaseOrderCode);
                result.OrderItems.Should().HaveCount(purchaseOrder.OrderItems.Count);
                result.TotalAmount.Should().Be(amount);
            }
        }

        [Fact]
        public async Task Supplier_name_validator_interceptor_executed()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            string sampleSupplierName = Supplier.GetSampleSupplierName();
            string testSupplierName = Supplier.GetTestSupplierName();
            string supplierCode = "N/A";
            string supplierEmail = "sample.test@test.com";
            Supplier supplier = Supplier.Create(sampleSupplierName, new SupplierDetails(supplierCode, supplierEmail));
            await sut.Supplier.AddAsync(supplier, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Name.Should().Be(testSupplierName);
        }

        [Fact]
        public async Task Purchase_order_code_validator_interceptor_executed()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            string samplePurchaseOrderCode = PurchaseOrder.GetSamplePurchaseOrderCode();
            string testPurchaseOrderCode = PurchaseOrder.GetTestPurchaseOrderCode();
            PurchaseOrder purchaseOrder = PurchaseOrder.Create(samplePurchaseOrderCode);
            purchaseOrder.AddOrderItem("Headset", 2, 20.25M);
            purchaseOrder.Validate();
            await sut.PurchaseOrder.AddAsync(purchaseOrder, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            PurchaseOrder result = await sut.PurchaseOrder
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.GlobalUId == purchaseOrder.GlobalUId, TestContext.Current.CancellationToken)
                .ConfigureAwait(false);

            // Assert
            result.Code.Should().Be(testPurchaseOrderCode);
        }
    }
}
