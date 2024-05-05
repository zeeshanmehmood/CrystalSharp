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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using CrystalSharp.Application;
using CrystalSharp.Application.Execution;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Application.CommandExecution.Commands;
using CrystalSharp.Tests.Common.Application.CommandExecution.Responses;
using CrystalSharp.Tests.Common.Application.NotificationExecution.Notifications;
using CrystalSharp.Tests.Common.Application.QueryExecution.Queries;
using CrystalSharp.Tests.Common.Application.QueryExecution.ReadModels;

namespace CrystalSharp.Tests.UnitTests.Application
{
    [Trait(TestSettings.Category, TestType.Unit)]
    public class ApplicationTests : IClassFixture<ApplicationTestFixture>
    {
        private readonly ApplicationTestFixture _fixture;

        public ApplicationTests(ApplicationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Command_executed()
        {
            // Arrange
            CreateOrderCommand command = new() { OrderCode = "ORDER-123" };
            ICommandExecutor commandExecutor = _fixture.CommandExecutor;

            // Act
            CommandExecutionResult<CreateOrderResponse> result = await commandExecutor.Execute(command, CancellationToken.None).ConfigureAwait(false);

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
            QueryExecutionResult<NameReadModel> result = await queryExecutor.Execute(query, CancellationToken.None).ConfigureAwait(false);

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
            QueryExecutionResult<IEnumerable<CustomerTypeReadModel>> result = await queryExecutor.Execute(query, CancellationToken.None).ConfigureAwait(false);

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
            await notificationPublisher.Publish(notification, CancellationToken.None).ConfigureAwait(false);

            // Assert
            notification.Status.Should().Be(status);
        }

        [Fact]
        public async Task Notification_published_to_multiple_registered_handlers()
        {
            // Arrange
            OrderDispatchedNotification notification = new();
            string[] status = new[] { "Email sent", "SMS sent" };
            INotificationPublisher notificationPublisher = _fixture.NotificationPublisher;

            // Act
            await notificationPublisher.Publish(notification, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                notification.Status.Contains(status[0]).Should().BeTrue();
                notification.Status.Contains(status[1]).Should().BeTrue();
            }
        }
    }
}
