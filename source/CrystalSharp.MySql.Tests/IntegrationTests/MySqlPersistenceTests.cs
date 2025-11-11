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
using CrystalSharp.Common.Utilities;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate;
using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate;
using CrystalSharp.Tests.Common.MySql.Infrastructure;

namespace CrystalSharp.MySql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MySqlIntegration)]
    public class MySqlPersistenceTests : IClassFixture<MySqlTestFixture>
    {
        private readonly MySqlTestFixture _testFixture;

        public MySqlPersistenceTests(MySqlTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Falcon", new SupplierInfo("FLC", "falcon@test.com"));
            await sut.Supplier.AddAsync(supplier, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_code_and_email_are_equal()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Cosmos", new SupplierInfo("CMS", "cosmos@test.com"));
            await sut.Supplier.AddAsync(supplier, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Cosmos");
                result.SupplierInfo.Code.Should().Be("CMS");
                result.SupplierInfo.Email.Should().Be("cosmos@test.com");
            }
        }

        [Fact]
        public async Task New_name_code_and_email_are_equal()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Three Star", new SupplierInfo("TSR", "three.star@test.com"));
            await sut.Supplier.AddAsync(supplier, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            supplier.ChangeName("Eagle");
            supplier.ChangeInfo(new SupplierInfo("EGL", "eagle@test.com"));
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Eagle");
                result.SupplierInfo.Code.Should().Be("EGL");
                result.SupplierInfo.Email.Should().Be("eagle@test.com");
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("Sunshine", new SupplierInfo("SSH", "sunshine@test.com"));
            await sut.Supplier.AddAsync(supplier, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            supplier.Delete();
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IMySqlDataContext sut = _testFixture.DataContext;
            Supplier supplier = Supplier.Create("QQ North", new SupplierInfo("QQN", "qq.north@test.com"));
            await sut.Supplier.AddAsync(supplier, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Supplier result = await sut.Supplier.SingleOrDefaultAsync(x => x.GlobalUId == supplier.GlobalUId, CancellationToken.None).ConfigureAwait(false);

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
            await sut.PurchaseOrder.AddAsync(purchaseOrder, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            PurchaseOrder result = await sut.PurchaseOrder
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(y => y.GlobalUId == purchaseOrder.GlobalUId, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Code.Should().Be(purchaseOrderCode);
                result.OrderItems.Should().HaveCount(purchaseOrder.OrderItems.Count);
                result.TotalAmount.Should().Be(amount);
            }
        }
    }
}
