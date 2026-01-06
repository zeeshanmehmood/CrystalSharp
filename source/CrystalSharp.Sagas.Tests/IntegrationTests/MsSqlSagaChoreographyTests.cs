using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MsSqlSagaChoreographyIntegration)]
    public class MsSqlSagaChoreographyTests(MsSqlSagaTestFixture fixture) : IClassFixture<MsSqlSagaTestFixture>
    {
        private readonly MsSqlSagaTestFixture _testFixture = fixture;

        [Fact]
        public async Task Saga_choreography_execution()
        {
            // Arrange
            ISagaTransactionExecutor sut = _testFixture.SagaTransactionExecutor;
            IInMemoryDataContext dataContext = _testFixture.DataContext;
            PlaceOrderTransaction transaction = new() { Product = "Chair", Quantity = 2, UnitPrice = 50, AmountPaid = 100 };

            // Act
            SagaTransactionResult sagaTransactionResult = await sut.Execute(transaction, CancellationToken.None).ConfigureAwait(false);
            Guid orderGlobalUId = sagaTransactionResult.CorrelationId;
            Order result = await dataContext.Order.SingleOrDefaultAsync(x => x.GlobalUId == orderGlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PaymentTransferred.Should().BeTrue();
                result.Delivered.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Saga_choreography_execution_rollback_with_compensation()
        {
            // Arrange
            ISagaTransactionExecutor sut = _testFixture.SagaTransactionExecutor;
            IInMemoryDataContext dataContext = _testFixture.DataContext;
            PlaceOrderTransaction transaction = new() { Product = "Laptop", Quantity = 1, UnitPrice = 450, AmountPaid = 400 };

            // Act
            SagaTransactionResult sagaTransactionResult = await sut.Execute(transaction, CancellationToken.None).ConfigureAwait(false);
            Guid orderGlobalUId = sagaTransactionResult.CorrelationId;
            Order result = await dataContext.Order.SingleOrDefaultAsync(x => x.GlobalUId == orderGlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.EntityStatus.Should().Be(EntityStatus.Deleted);
            }
        }
    }
}
