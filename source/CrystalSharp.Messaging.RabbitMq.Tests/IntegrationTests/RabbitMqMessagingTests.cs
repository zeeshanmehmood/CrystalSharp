using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Messaging.Distributed.Models;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Messaging.Data;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Messaging.RabbitMq.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.RabbitMqIntegration)]
    public class RabbitMqMessagingTests(RabbitMqTestFixture fixture) : IClassFixture<RabbitMqTestFixture>
    {
        private readonly RabbitMqTestFixture _testFixture = fixture;

        [Fact]
        public async Task Publish_object_to_exchange()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralExchangeMessage<Customer> message = new()
            {
                Exchange = new GeneralExchange
                {
                    Name = "test-object-message-exchange",
                    RoutingKey = "test-object-message-exchange-routing-key",
                    Queues = [
                        new()
                        {
                            Name = "test-object-message-queue",
                            RoutingKeys = ["test-object-message-exchange-routing-key"]
                        }
                    ]
                },
                Body = Customer.CreateObject("John Walker", 4, false)
            };

            // Act
            await sut.PublishObject<Customer>(message, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            message.Exchange.Should().NotBeNull();
        }

        [Fact]
        public async Task Publish_json_to_exchange()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralExchangeMessage message = new()
            {
                Exchange = new GeneralExchange
                {
                    Name = "test-json-message-exchange",
                    RoutingKey = "test-json-message-exchange-routing-key",
                    Queues = [
                        new()
                        {
                            Name = "test-json-message-queue",
                            RoutingKeys = ["test-json-message-exchange-routing-key"]
                        }
                    ]
                },
                Body = Customer.CreateJson("George Wilson", 5, true)
            };

            // Act
            await sut.PublishJson(message, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            message.Exchange.Should().NotBeNull();
        }

        [Fact]
        public async Task Publish_string_to_exchange()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralExchangeMessage message = new()
            {
                Exchange = new GeneralExchange
                {
                    Name = "test-string-message-exchange",
                    RoutingKey = "test-string-message-exchange-routing-key",
                    Queues = [
                        new()
                        {
                            Name = "test-string-message-queue",
                            RoutingKeys = ["test-string-message-exchange-routing-key"]
                        }
                    ]
                },
                Body = "Test message for exchange."
            };

            // Act
            await sut.PublishString(message, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            message.Exchange.Should().NotBeNull();
        }

        [Fact]
        public async Task Send_object_to_queue()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage<Customer> message = new()
            {
                Queue = new GeneralQueue { Name = "test-send-object-message-queue" },
                Body = Customer.CreateObject("John Webb", 3, true)
            };

            // Act
            await sut.SendObject<Customer>(message, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            message.Queue.Should().NotBeNull();
        }

        [Fact]
        public async Task Send_json_to_queue()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage message = new()
            {
                Queue = new GeneralQueue { Name = "test-send-json-message-queue" },
                Body = Customer.CreateJson("Mark Anderson", 1, false)
            };

            // Act
            await sut.SendJson(message, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            message.Queue.Should().NotBeNull();
        }

        [Fact]
        public async Task Send_string_to_queue()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage message = new()
            {
                Queue = new GeneralQueue { Name = "test-send-string-message-queue" },
                Body = "Test string message for queue"
            };

            // Act
            await sut.SendString(message, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            message.Queue.Should().NotBeNull();
        }

        [Fact]
        public async Task Consume_message_from_queue()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage message = new()
            {
                Queue = new GeneralQueue { Name = "test-send-string-message-consumer-queue" },
                Body = "Test message for queue"
            };
            string result = string.Empty;
            IList<string> queues = ["test-send-string-message-consumer-queue"];
            GeneralConsumer consumer = new()
            {
                Queues = queues,
                Action = m => { result = m; }
            };
            await sut.SendString(message, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Act
            await sut.StartConsuming(consumer, TestContext.Current.CancellationToken).ConfigureAwait(false);
            Thread.Sleep(1000);
            await sut.StopConsuming(TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            result.Should().Be(message.Body);
        }
    }
}
