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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;

namespace CrystalSharp.Sagas.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.PostgreSqlSagaChoreographyIntegration)]
    public class PostgreSqlSagaChoreographyTests : IClassFixture<PostgreSqlSagaTestFixture>
    {
        private readonly PostgreSqlSagaTestFixture _testFixture;

        public PostgreSqlSagaChoreographyTests(PostgreSqlSagaTestFixture fixture)
        {
            _testFixture = fixture;
        }

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
