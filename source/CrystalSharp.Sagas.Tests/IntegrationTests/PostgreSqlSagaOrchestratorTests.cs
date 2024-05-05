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
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using CrystalSharp.Tests.Common.Sagas.Orchestration.TripOrchestration;

namespace CrystalSharp.Sagas.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.PostgreSqlSagaOrchestratorIntegration)]
    public class PostgreSqlSagaOrchestratorTests : IClassFixture<PostgreSqlSagaTestFixture>
    {
        private readonly PostgreSqlSagaTestFixture _testFixture;

        public PostgreSqlSagaOrchestratorTests(PostgreSqlSagaTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Saga_orchestrator_execution()
        {
            // Arrange
            ISagaTransactionExecutor sut = _testFixture.SagaTransactionExecutor;
            IInMemoryDataContext dataContext = _testFixture.DataContext;
            PlanTripTransaction transaction = new()
            {
                Name = "Europe Tour",
                Hotel = "Grand City Hotel",
                ReservationAmount = 2000,
                HotelReservationPaidByCustomer = 2000,
                Car = "Toyota Camry 2021",
                Rent = 200,
                CarRentPaidByCustomer = 200,
                Flight = "Flight SOA123",
                Fare = 1500,
                FlightFarePaidByCustomer = 1500
            };

            // Act
            SagaTransactionResult sagaTransactionResult = await sut.Execute(transaction, CancellationToken.None).ConfigureAwait(false);
            Guid correlationId = sagaTransactionResult.CorrelationId;
            Trip result = await dataContext.Trip.SingleOrDefaultAsync(x => x.CorrelationId == correlationId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.CorrelationId.Equals(correlationId);
                result.HotelReservationConfirmed.Should().BeTrue();
                result.CarReserved.Should().BeTrue();
                result.FlightConfirmed.Should().BeTrue();
                result.Confirm.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Saga_orchestrator_rollback_with_compensation()
        {
            // Arrange
            ISagaTransactionExecutor sut = _testFixture.SagaTransactionExecutor;
            IInMemoryDataContext dataContext = _testFixture.DataContext;
            PlanTripTransaction transaction = new()
            {
                Name = "Eastern Europe Tour",
                Hotel = "Eastern Hotel",
                ReservationAmount = 1500,
                HotelReservationPaidByCustomer = 1500,
                Car = "Toyota Camry 2021",
                Rent = 100,
                CarRentPaidByCustomer = 100,
                Flight = "Flight SOA456",
                Fare = 1000,
                FlightFarePaidByCustomer = 950
            };

            // Act
            SagaTransactionResult sagaTransactionResult = await sut.Execute(transaction, CancellationToken.None).ConfigureAwait(false);
            Guid correlationId = sagaTransactionResult.CorrelationId;
            Trip result = await dataContext.Trip.SingleOrDefaultAsync(x => x.CorrelationId == correlationId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.CorrelationId.Equals(correlationId);
                result.HotelReservationConfirmed.Should().BeFalse();
                result.CarReserved.Should().BeFalse();
                result.FlightConfirmed.Should().BeFalse();
                result.Confirm.Should().BeFalse();
            }
        }
    }
}
