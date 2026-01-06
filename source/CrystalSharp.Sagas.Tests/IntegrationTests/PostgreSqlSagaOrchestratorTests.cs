using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using CrystalSharp.Tests.Common.Sagas.Orchestration.TripOrchestration;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.PostgreSqlSagaOrchestratorIntegration)]
    public class PostgreSqlSagaOrchestratorTests(PostgreSqlSagaTestFixture fixture) : IClassFixture<PostgreSqlSagaTestFixture>
    {
        private readonly PostgreSqlSagaTestFixture _testFixture = fixture;

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
