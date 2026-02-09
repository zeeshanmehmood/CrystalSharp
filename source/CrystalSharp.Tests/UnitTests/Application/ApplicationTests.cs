using CrystalSharp.Application;
using CrystalSharp.Application.Execution;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Application.CommandExecution.Commands;
using CrystalSharp.Tests.Common.Application.CommandExecution.Responses;
using CrystalSharp.Tests.Common.Application.NotificationExecution.Notifications;
using CrystalSharp.Tests.Common.Application.QueryExecution.Queries;
using CrystalSharp.Tests.Common.Application.QueryExecution.ReadModels;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.UnitTests.Application
{
    [Trait(TestSettings.Category, TestType.Unit)]
    public class ApplicationTests(ApplicationTestFixture fixture) : IClassFixture<ApplicationTestFixture>
    {
        private readonly ApplicationTestFixture _fixture = fixture;

        [Fact]
        public async Task Command_executed()
        {
            // Arrange
            CreateOrderCommand command = new() { OrderCode = "ORDER-123" };
            ICommandExecutor commandExecutor = _fixture.CommandExecutor;

            // Act
            CommandExecutionResult<CreateOrderResponse> result = await commandExecutor.Execute(command, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Success.Should().BeTrue();
                result.Data.Should().NotBeNull();
                result.Data.Id.Should().NotBe(Guid.Empty);
                result.Data.Success.Should().BeTrue();
                result.Data.OrderCode.Should().Be(command.OrderCode);
            }
        }

        [Fact]
        public async Task Query_executed()
        {
            // Arrange
            string firstName = "Nancy";
            string lastName = "Fuller";
            string fullName = $"{firstName} {lastName}";
            ConsolidateNameQuery query = new() { FirstName = firstName, LastName = lastName };
            IQueryExecutor queryExecutor = _fixture.QueryExecutor;

            // Act
            QueryExecutionResult<NameReadModel> result = await queryExecutor.Execute(query, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Success.Should().BeTrue();
                result.Data.Should().NotBeNull();
                result.Data.Name.Should().Be(fullName);
            }
        }

        [Fact]
        public async Task Query_with_enumerated_data()
        {
            // Arrange
            string firstCustomerType = "Gold";
            string secondCustomerType = "Silver";
            int expectedCount = 2;
            ConsolidateCustomerTypeQuery query = new() { FirstCustomerType = firstCustomerType, SecondCustomerType = secondCustomerType };
            IQueryExecutor queryExecutor = _fixture.QueryExecutor;

            // Act
            QueryExecutionResult<IEnumerable<CustomerTypeReadModel>> result = await queryExecutor.Execute(query, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Success.Should().BeTrue();
                result.Data.Should().NotBeNull();
                result.Data.Should().HaveCount(expectedCount);
            }
        }

        [Fact]
        public async Task Notification_published_to_single_registered_handler()
        {
            // Arrange
            OrderCreatedNotification notification = new();
            string status = "Order created";
            INotificationPublisher notificationPublisher = _fixture.NotificationPublisher;

            // Act
            await notificationPublisher.Publish(notification, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            notification.Status.Should().Be(status);
        }

        [Fact]
        public async Task Notification_published_to_multiple_registered_handlers()
        {
            // Arrange
            OrderDispatchedNotification notification = new();
            string[] status = ["Email sent", "SMS sent"];
            INotificationPublisher notificationPublisher = _fixture.NotificationPublisher;

            // Act
            await notificationPublisher.Publish(notification, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                notification.Status.Contains(status[0]).Should().BeTrue();
                notification.Status.Contains(status[1]).Should().BeTrue();
            }
        }
    }
}
