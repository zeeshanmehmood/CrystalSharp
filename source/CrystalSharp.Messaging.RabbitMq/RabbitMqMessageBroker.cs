using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Messaging.Distributed.Exceptions;
using CrystalSharp.Messaging.Distributed.Models;
using CrystalSharp.Messaging.RabbitMq.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Messaging.RabbitMq
{
    public sealed class RabbitMqMessageBroker(RabbitMqSettings settings, IRabbitMqConnectionFactory connectionFactory) : IMessageBroker
    {
        private readonly RabbitMqSettings _settings = settings;
        private readonly RabbitMqChannelConfiguration _channelConfiguration = null;
        private readonly IRabbitMqConnectionFactory _connectionFactory = connectionFactory;
        
        private AsyncEventingBasicConsumer _consumer;
        private bool _consuming = false;
        private IEnumerable<string> _consumerTags = [];
        private Action<string> _action;

        public IConnection Connection { get; private set; }
        public IChannel ProducerChannel { get; private set; }
        public IChannel ConsumerChannel { get; private set; }

        public RabbitMqMessageBroker(
            RabbitMqSettings settings,
            RabbitMqChannelConfiguration channelConfiguration,
            IRabbitMqConnectionFactory connectionFactory)
            : this(settings, connectionFactory)
        {
            _channelConfiguration = channelConfiguration;
        }

        public async Task PublishObject<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default) where T : class
        {
            await ValidateConnectionAndChannels(cancellationToken).ConfigureAwait(false);
            await PublishObjectToExchange<T>(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task PublishJson(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAndChannels(cancellationToken).ConfigureAwait(false);
            await PublishJsonToExchange(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task PublishString(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAndChannels(cancellationToken).ConfigureAwait(false);
            await PublishStringToExchange(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendObject<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default) where T : class
        {
            await ValidateConnectionAndChannels(cancellationToken).ConfigureAwait(false);
            await SendObjectToQueue<T>(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendJson(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAndChannels(cancellationToken).ConfigureAwait(false);
            await SendJsonToQueue(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendString(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAndChannels(cancellationToken).ConfigureAwait(false);
            await SendStringToQueue(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task StartConsuming(IConsumer consumer, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAndChannels(cancellationToken).ConfigureAwait(false);
            EnsureConsumingChannelIsNotNull();
            ValidateConsumer(consumer);

            if (_consuming)
            {
                return;
            }

            _consumer.ReceivedAsync += ConsumerOnReceived;
            _consuming = true;
            _action = consumer.Action;
            List<string> tags = [];

            foreach (string queue in consumer.Queues)
            {
                string consumerTag = await ConsumerChannel.BasicConsumeAsync(queue, false, _consumer, cancellationToken).ConfigureAwait(false);

                tags.Add(consumerTag);
            }

            _consumerTags = tags;
        }

        public async Task StopConsuming(CancellationToken cancellationToken = default)
        {
            EnsureConsumingChannelIsNotNull();

            if (!_consuming)
            {
                return;
            }

            _consumer.ReceivedAsync -= ConsumerOnReceived;
            _consuming = false;

            if (_consumerTags.HasAny())
            {
                foreach (string consumerTag in _consumerTags)
                {
                    await ConsumerChannel.BasicCancelAsync(consumerTag, false, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task Disconnect(CancellationToken cancellationToken = default)
        {
            EnsureProducingChannelIsNotNull();
            EnsureConsumingChannelIsNotNull();

            if (ProducerChannel.IsOpen)
            {
                ProducerChannel.CallbackExceptionAsync -= HandleChannelCallbackException;

                await ProducerChannel.CloseAsync(200, "Producer channel closed", cancellationToken).ConfigureAwait(false);
            }

            if (ConsumerChannel.IsOpen)
            {
                ConsumerChannel.CallbackExceptionAsync -= HandleChannelCallbackException;

                await ConsumerChannel.CloseAsync(200, "Consumer channel closed", cancellationToken).ConfigureAwait(false);
            }

            if (Connection.IsOpen)
            {
                Connection.CallbackExceptionAsync -= HandleConnectionCallbackException;
                Connection.ConnectionRecoveryErrorAsync -= HandleConnectionRecoveryError;

                await Connection.CloseAsync(200, "Connection closed", cancellationToken).ConfigureAwait(false);
            }

            await ProducerChannel.DisposeAsync().ConfigureAwait(false);
            await ConsumerChannel.DisposeAsync().ConfigureAwait(false);
            await Connection.DisposeAsync().ConfigureAwait(false);
        }

        private async Task ValidateConnectionAndChannels(CancellationToken cancellationToken = default)
        {
            if (Connection is null || !Connection.IsOpen)
            {
                Connection = await _connectionFactory.CreateConnection(_settings, cancellationToken).ConfigureAwait(false);
            }

            if (Connection is not null)
            {
                Connection.CallbackExceptionAsync += HandleConnectionCallbackException;
                Connection.ConnectionRecoveryErrorAsync += HandleConnectionRecoveryError;

                CreateChannelOptions channelOptions = null;

                if (_channelConfiguration is not null)
                {
                    channelOptions = new(
                        _channelConfiguration.PublisherConfirmationsEnabled,
                        _channelConfiguration.PublisherConfirmationTrackingEnabled,
                        _channelConfiguration.OutstandingPublisherConfirmationsRateLimiter,
                        _channelConfiguration.ConsumerDispatchConcurrency);
                }

                if (ProducerChannel is null || !ProducerChannel.IsOpen)
                {
                    ProducerChannel = await Connection.CreateChannelAsync(channelOptions, cancellationToken).ConfigureAwait(false);
                    ProducerChannel.CallbackExceptionAsync += HandleChannelCallbackException;
                }

                if (ConsumerChannel is null || !ConsumerChannel.IsOpen)
                {
                    ConsumerChannel = await Connection.CreateChannelAsync(channelOptions, cancellationToken).ConfigureAwait(false);
                    ConsumerChannel.CallbackExceptionAsync += HandleChannelCallbackException;

                    _consumer = new(ConsumerChannel);
                }
            }
        }

        private async Task HandleConnectionCallbackException(object sender, CallbackExceptionEventArgs @event)
        {
            if (@event is null)
            {
                return;
            }

            throw @event.Exception;
        }

        private async Task HandleConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs @event)
        {
            if (@event is null)
            {
                return;
            }

            throw @event.Exception;
        }

        private async Task HandleChannelCallbackException(object sender, CallbackExceptionEventArgs @event)
        {
            if (@event is null)
            {
                return;
            }

            throw @event.Exception;
        }

        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
        {
            string messageFromQueue = ConvertBytesToBody(@event.Body.ToArray());

            _action(messageFromQueue);
            await ConsumerChannel.BasicAckAsync(@event.DeliveryTag, false).ConfigureAwait(false);
        }

        private async Task DeclareQueue(IChannel channel, GeneralQueue queue, CancellationToken cancellationToken = default)
        {
            await channel.QueueDeclareAsync(
                queue: queue.Name,
                durable: queue.Durable,
                exclusive: queue.Exclusive,
                autoDelete: queue.AutoDelete,
                arguments: queue.Arguments,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task BindQueue(IChannel channel, string exchange, GeneralQueue queue, CancellationToken cancellationToken = default)
        {
            if (queue.RoutingKeys.Count != 0)
            {
                foreach (string routingKey in queue.RoutingKeys)
                {
                    await DeclareQueue(channel, queue, cancellationToken).ConfigureAwait(false);
                    await channel.QueueBindAsync(queue: queue.Name, exchange: exchange, routingKey: routingKey, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await DeclareQueue(channel, queue, cancellationToken).ConfigureAwait(false);
                await channel.QueueBindAsync(queue: queue.Name, exchange: exchange, routingKey: queue.Name, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SetQueue(IChannel channel, GeneralQueue queue, CancellationToken cancellationToken = default)
        {
            if (queue.Exchange.IsValidString())
            {
                await BindQueue(channel, queue.Exchange, queue, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await DeclareQueue(channel, queue, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SetExchange(IChannel channel, GeneralExchange exchange, CancellationToken cancellationToken = default)
        {
            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: exchange.Type,
                durable: exchange.Durable,
                autoDelete: exchange.AutoDelete,
                arguments: exchange.Arguments,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (exchange.Queues.HasAny())
            {
                foreach (GeneralQueue queue in exchange.Queues)
                {
                    await BindQueue(channel, exchange.Name, queue, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private BasicProperties CreateBasicProperties(string contentType = "")
        {
            BasicProperties basicProperties = new() { Persistent = true };

            if (contentType.IsValidString())
            {
                basicProperties.ContentType = contentType;
            }

            return basicProperties;
        }

        private byte[] ConvertBodyToBytes(string body)
        {
            return Encoding.UTF8.GetBytes(body);
        }

        private string ConvertBytesToBody(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private async Task SendToExchange(
            string exchange,
            string routingKey,
            bool mandatory,
            BasicProperties basicProperties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default)
        {
            await ProducerChannel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: mandatory,
                basicProperties: basicProperties,
                body: body,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SendToQueue(
            string exchange,
            string queue,
            bool mandatory,
            BasicProperties basicProperties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default)
        {
            await ProducerChannel.BasicPublishAsync(
                exchange: exchange,
                routingKey: queue,
                mandatory: mandatory,
                basicProperties: basicProperties,
                body: body,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task PublishObjectToExchange<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default) where T : class
        {
            GeneralExchangeMessage<T> message = distributedMessage as GeneralExchangeMessage<T>;

            EnsureProducingChannelIsNotNull();
            ValidateExchange(message.Exchange);

            bool mandatory = true;
            string json = Serializer.Serialize(message.Body);
            byte[] body = ConvertBodyToBytes(json);
            BasicProperties basicProperties = CreateBasicProperties("application/json");

            await SetExchange(ProducerChannel, message.Exchange, cancellationToken).ConfigureAwait(false);
            await SendToExchange(
                message.Exchange.Name,
                message.Exchange.RoutingKey,
                mandatory,
                basicProperties,
                body,
                cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task PublishJsonToExchange(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            GeneralExchangeMessage message = distributedMessage as GeneralExchangeMessage;

            EnsureProducingChannelIsNotNull();
            ValidateExchange(message.Exchange);

            bool mandatory = true;
            byte[] body = ConvertBodyToBytes(message.Body);
            BasicProperties basicProperties = CreateBasicProperties("application/json");

            await SetExchange(ProducerChannel, message.Exchange, cancellationToken).ConfigureAwait(false);
            await SendToExchange(
                message.Exchange.Name,
                message.Exchange.RoutingKey,
                mandatory,
                basicProperties,
                body,
                cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task PublishStringToExchange(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            GeneralExchangeMessage message = distributedMessage as GeneralExchangeMessage;

            EnsureProducingChannelIsNotNull();
            ValidateExchange(message.Exchange);

            bool mandatory = true;
            byte[] body = ConvertBodyToBytes(message.Body);
            BasicProperties basicProperties = CreateBasicProperties();

            await SetExchange(ProducerChannel, message.Exchange, cancellationToken).ConfigureAwait(false);
            await SendToExchange(
                message.Exchange.Name,
                message.Exchange.RoutingKey,
                mandatory,
                basicProperties,
                body,
                cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SendObjectToQueue<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default) where T : class
        {
            GeneralQueueMessage<T> message = distributedMessage as GeneralQueueMessage<T>;

            EnsureProducingChannelIsNotNull();
            ValidateQueue(message.Queue);

            bool mandatory = true;
            string json = Serializer.Serialize(message.Body);
            byte[] body = ConvertBodyToBytes(json);
            BasicProperties basicProperties = CreateBasicProperties("application/json");

            await SetQueue(ProducerChannel, message.Queue, cancellationToken).ConfigureAwait(false);
            await SendToQueue(
                message.Queue.Exchange,
                message.Queue.Name,
                mandatory,
                basicProperties,
                body,
                cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SendJsonToQueue(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            GeneralQueueMessage message = distributedMessage as GeneralQueueMessage;

            EnsureProducingChannelIsNotNull();
            ValidateQueue(message.Queue);

            bool mandatory = true;
            byte[] body = ConvertBodyToBytes(message.Body);
            BasicProperties basicProperties = CreateBasicProperties("application/json");

            await SetQueue(ProducerChannel, message.Queue, cancellationToken).ConfigureAwait(false);
            await SendToQueue(
                message.Queue.Exchange,
                message.Queue.Name,
                mandatory,
                basicProperties,
                body,
                cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SendStringToQueue(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            GeneralQueueMessage message = distributedMessage as GeneralQueueMessage;

            EnsureProducingChannelIsNotNull();
            ValidateQueue(message.Queue);

            bool mandatory = true;
            byte[] body = ConvertBodyToBytes(message.Body);
            BasicProperties basicProperties = CreateBasicProperties();

            await SetQueue(ProducerChannel, message.Queue, cancellationToken).ConfigureAwait(false);
            await SendToQueue(
                message.Queue.Exchange,
                message.Queue.Name,
                mandatory,
                basicProperties,
                body,
                cancellationToken)
                .ConfigureAwait(false);
        }

        private void EnsureProducingChannelIsNotNull()
        {
            if (ProducerChannel is null)
            {
                throw new ProducingChannelIsNullException(ReservedErrorCode.SystemError, "Producing channel is null.");
            }
        }

        private void EnsureConsumingChannelIsNotNull()
        {
            if (ConsumerChannel is null)
            {
                throw new ConsumingChannelIsNullException(ReservedErrorCode.SystemError, "Consuming channel is null.");
            }
        }

        private void ValidateExchange(GeneralExchange exchange)
        {
            if (exchange is null)
            {
                throw new ExchangeIsNullException(ReservedErrorCode.SystemError, "Exchange is null.");
            }

            if (string.IsNullOrEmpty(exchange.Name))
            {
                throw new ExchangeIsNullException(ReservedErrorCode.SystemError, "Exchange name is null or empty.");
            }
        }

        private void ValidateQueue(GeneralQueue queue)
        {
            if (queue is null)
            {
                throw new QueueIsNullException(ReservedErrorCode.SystemError, "Queue is null.");
            }

            if (string.IsNullOrEmpty(queue.Name))
            {
                throw new QueueIsNullException(ReservedErrorCode.SystemError, "Queue name is null or empty.");
            }
        }

        private void ValidateConsumer(IConsumer consumer)
        {
            if (consumer is null)
            {
                throw new ConsumerIsNullException(ReservedErrorCode.SystemError, "Consumer is null.");
            }

            if (consumer.Queues.Count == 0)
            {
                throw new QueueIsNullException(ReservedErrorCode.SystemError, "Consumer queues are empty.");
            }

            if (consumer.Action is null)
            {
                throw new ConsumerActionIsNullException(ReservedErrorCode.SystemError, "Consumer action is null.");
            }
        }
    }
}
