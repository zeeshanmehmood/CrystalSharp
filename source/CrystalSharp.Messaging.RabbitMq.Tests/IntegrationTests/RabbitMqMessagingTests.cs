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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Messaging.Distributed.Models;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Messaging.Data;

namespace CrystalSharp.Messaging.RabbitMq.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.RabbitMqIntegration)]
    public class RabbitMqMessagingTests : IClassFixture<RabbitMqTestFixture>
    {
        private readonly RabbitMqTestFixture _testFixture;

        public RabbitMqMessagingTests(RabbitMqTestFixture fixture)
        {
            _testFixture = fixture;
        }

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
                    Queues = new List<GeneralQueue>
                    {
                        new GeneralQueue
                        {
                            Name = "test-object-message-queue",
                            RoutingKeys = new HashSet<string> { "test-object-message-exchange-routing-key" }
                        }
                    }
                }
            };
            message.Body = Customer.CreateObject("John Walker", 4, false);

            // Act
            await sut.PublishObject<Customer>(message, CancellationToken.None).ConfigureAwait(false);

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
                    Queues = new List<GeneralQueue>
                    {
                        new GeneralQueue
                        {
                            Name = "test-json-message-queue",
                            RoutingKeys = new HashSet<string> { "test-json-message-exchange-routing-key" }
                        }
                    }
                }
            };
            message.Body = Customer.CreateJson("George Wilson", 5, true);

            // Act
            await sut.PublishJson(message, CancellationToken.None).ConfigureAwait(false);

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
                    Queues = new List<GeneralQueue>
                    {
                        new GeneralQueue
                        {
                            Name = "test-string-message-queue",
                            RoutingKeys = new HashSet<string> { "test-string-message-exchange-routing-key" }
                        }
                    }
                }
            };
            message.Body = "Test message for exchange.";

            // Act
            await sut.PublishString(message, CancellationToken.None).ConfigureAwait(false);

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
                Queue = new GeneralQueue { Name = "test-send-object-message-queue" }
            };
            message.Body = Customer.CreateObject("John Webb", 3, true);

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
                Queue = new GeneralQueue { Name = "test-send-json-message-queue" }
            };
            message.Body = Customer.CreateJson("Mark Anderson", 1, false);

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
                Queue = new GeneralQueue { Name = "test-send-string-message-queue" }
            };
            message.Body = "Test string message for queue";

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
                Queue = new GeneralQueue { Name = "test-send-string-message-consumer-queue" }
            };
            message.Body = "Test message for queue";
            string result = string.Empty;
            IList<string> queues = new List<string>() { "test-send-string-message-consumer-queue" };
            GeneralConsumer consumer = new()
            {
                Queues = queues,
                Action = (string m) => { result = m; }
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
