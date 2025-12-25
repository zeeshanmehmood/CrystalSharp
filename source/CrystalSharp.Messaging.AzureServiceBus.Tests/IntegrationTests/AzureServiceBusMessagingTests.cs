using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Messaging.Distributed.Models;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Messaging.Data;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Messaging.AzureServiceBus.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.AzureServiceBusIntegration)]
    public class AzureServiceBusMessagingTests(AzureServiceBusTestFixture fixture) : IClassFixture<AzureServiceBusTestFixture>
    {
        private readonly AzureServiceBusTestFixture _testFixture = fixture;

        [Fact]
        public async Task Publish_object_to_topic()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage<Customer> message = new()
            {
                Queue = new GeneralQueue { Exchange = "test-object-message-topic" },
                Body = Customer.CreateObject("John Walker", 4, false)
            };

            // Act
            await sut.PublishObject<Customer>(message, CancellationToken.None).ConfigureAwait(false);

            // Assert
            message.Queue.Exchange.Should().NotBeNull();
        }

        [Fact]
        public async Task Publish_json_to_topic()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage message = new()
            {
                Queue = new GeneralQueue { Exchange = "test-json-message-topic" },
                Body = Customer.CreateJson("George Wilson", 5, true)
            };

            // Act
            await sut.PublishJson(message, CancellationToken.None).ConfigureAwait(false);

            // Assert
            message.Queue.Exchange.Should().NotBeNull();
        }

        [Fact]
        public async Task Publish_string_to_topic()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage message = new()
            {
                Queue = new GeneralQueue { Exchange = "test-string-message-topic" },
                Body = "Test message for topic."
            };

            // Act
            await sut.PublishString(message, CancellationToken.None).ConfigureAwait(false);

            // Assert
            message.Queue.Exchange.Should().NotBeNull();
        }

        [Fact]
        public async Task Send_object_to_queue()
        {
            // Arrange
            IMessageBroker sut = _testFixture.MessageBroker;
            GeneralQueueMessage<Customer> message = new()
            {
                Queue = new GeneralQueue
                {
                    Name = "test-send-object-message-queue",
                    Arguments = new Dictionary<string, object> { { "CustomerType", "Basic" } }
                },
                Body = Customer.CreateObject("John Webb", 3, true)
            };

            // Act
            await sut.SendObject<Customer>(message, CancellationToken.None).ConfigureAwait(false);

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
            await sut.SendJson(message, CancellationToken.None).ConfigureAwait(false);

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
            await sut.SendString(message, CancellationToken.None).ConfigureAwait(false);

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
            List<string> queues = ["test-send-string-message-consumer-queue"];
            GeneralConsumer consumer = new()
            {
                Queues = queues,
                Action = m => { result = m; }
            };
            await sut.SendString(message, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.StartConsuming(consumer).ConfigureAwait(false);
            Thread.Sleep(1000);
            await sut.StopConsuming().ConfigureAwait(false);

            // Assert
            result.Should().Be(message.Body);
        }
    }
}
