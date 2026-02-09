using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Serialization;
using CrystalSharp.Common.Settings;
using CrystalSharp.Messaging.AzureServiceBus.Configuration;
using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Messaging.Distributed.Exceptions;
using CrystalSharp.Messaging.Distributed.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Messaging.AzureServiceBus
{
    public class AzureServiceBusMessageBroker : IMessageBroker, IDisposable, IAsyncDisposable
    {
        private readonly AzureServiceBusSettings _azureServiceBusSettings;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusAdministrationClient _serviceBusAdminitratorClient;
        private IEnumerable<ServiceBusProcessor> _consumers = [];
        private bool _consuming = false;
        private Action<string> _action;
        private bool _isDisposed = false;

        public AzureServiceBusMessageBroker(AzureServiceBusSettings settings)
        {
            _azureServiceBusSettings = settings;
            _serviceBusClient = new(_azureServiceBusSettings.ConnectionString);
            _serviceBusAdminitratorClient = new(_azureServiceBusSettings.ConnectionString);
        }

        public async Task PublishObject<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default)
            where T : class
        {
            await SendObjectToTopicOrQueue<T>(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task PublishJson(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await SendStringToTopicOrQueue(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task PublishString(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await SendStringToTopicOrQueue(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendObject<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default)
            where T : class
        {
            await SendObjectToTopicOrQueue<T>(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendJson(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await SendStringToTopicOrQueue(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendString(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            await SendStringToTopicOrQueue(distributedMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task StartConsuming(IConsumer consumer, CancellationToken cancellationToken = default)
        {
            ValidateConsumer(consumer);

            if (_consuming)
            {
                return;
            }

            _consuming = true;
            _action = consumer.Action;
            List<ServiceBusProcessor> tags = [];

            foreach (string channel in consumer.Queues)
            {
                ServiceBusProcessor serviceBusProcessor = _serviceBusClient.CreateProcessor(channel);
                serviceBusProcessor.ProcessMessageAsync += ConsumerOnReceived;
                serviceBusProcessor.ProcessErrorAsync += ConsumerOnReceivedError;

                await serviceBusProcessor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);

                tags.Add(serviceBusProcessor);
            }

            _consumers = tags;
        }

        public async Task StopConsuming(CancellationToken cancellationToken = default)
        {
            if (!_consuming)
            {
                return;
            }

            if (_consumers.HasAny())
            {
                foreach (ServiceBusProcessor serviceBusProcessor in _consumers)
                {
                    await serviceBusProcessor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);

                    serviceBusProcessor.ProcessMessageAsync -= ConsumerOnReceived;
                    serviceBusProcessor.ProcessErrorAsync -= ConsumerOnReceivedError;

                    await serviceBusProcessor.DisposeAsync();
                }
            }

            _consuming = false;
        }

        public async Task Disconnect(CancellationToken cancellationToken = default)
        {
            Dispose();
        }

        public void Dispose()
        {
            Task.Run(async () => await DisposeAsync());
        }

        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                await _serviceBusClient.DisposeAsync();
            }
        }

        private async Task CreateTopicIfNotExists(string topic, CancellationToken cancellationToken = default)
        {
            bool exists = await _serviceBusAdminitratorClient.TopicExistsAsync(topic, cancellationToken).ConfigureAwait(false);

            if (!exists)
            {
                await _serviceBusAdminitratorClient.CreateTopicAsync(topic, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task CreateQueueIfNotExists(string queue, CancellationToken cancellationToken = default)
        {
            bool exists = await _serviceBusAdminitratorClient.QueueExistsAsync(queue, cancellationToken).ConfigureAwait(false);

            if (!exists)
            {
                await _serviceBusAdminitratorClient.CreateQueueAsync(queue, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task CreateTopicOrQueueIfNotExists(QueueMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Queue.Exchange.IsValidString())
            {
                await CreateTopicIfNotExists(message.Queue.Exchange, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await CreateQueueIfNotExists(message.Queue.Name, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SendObjectToTopicOrQueue<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default)
            where T : class
        {
            GeneralQueueMessage<T> message = distributedMessage as GeneralQueueMessage<T>;

            ValidateProducingChannel(message);
            await CreateTopicOrQueueIfNotExists(message, cancellationToken).ConfigureAwait(false);

            string channel = message.Queue.Exchange.IsValidString() ? message.Queue.Exchange : message.Queue.Name;
            string body = Serializer.Serialize(message.Body);

            await Send(channel, body, message.Queue.Arguments, cancellationToken).ConfigureAwait(false);
        }

        private async Task SendStringToTopicOrQueue(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default)
        {
            GeneralQueueMessage message = distributedMessage as GeneralQueueMessage;

            ValidateProducingChannel(message);
            await CreateTopicOrQueueIfNotExists(message, cancellationToken).ConfigureAwait(false);

            string channel = message.Queue.Exchange.IsValidString() ? message.Queue.Exchange : message.Queue.Name;

            await Send(channel, message.Body, message.Queue.Arguments, cancellationToken).ConfigureAwait(false);
        }

        private async Task Send(
            string channel,
            string body,
            IDictionary<string, object> customProperties,
            CancellationToken cancellationToken = default)
        {
            ServiceBusSender serviceBusSender = _serviceBusClient.CreateSender(channel);
            ServiceBusMessage serviceBusMessage = new(body);

            if (customProperties is not null && customProperties.HasAny())
            {
                foreach (string key in customProperties.Keys)
                {
                    serviceBusMessage.ApplicationProperties.Add(key, customProperties[key]);
                }
            }

            await serviceBusSender.SendMessageAsync(serviceBusMessage, cancellationToken).ConfigureAwait(false);
            await serviceBusSender.DisposeAsync();
        }

        private async Task ConsumerOnReceived(ProcessMessageEventArgs eventArgs)
        {
            string messageFromQueue = eventArgs.Message.Body.ToString();

            _action(messageFromQueue);
            await eventArgs.CompleteMessageAsync(eventArgs.Message);
        }

        private async Task ConsumerOnReceivedError(ProcessErrorEventArgs eventArgs)
        {
            await Task.CompletedTask;

            string errorMessage = eventArgs.Exception.Message;

            throw new ConsumerMessageException(ReservedErrorCode.SystemError, errorMessage, eventArgs.Exception);
        }

        private void ValidateProducingChannel(GeneralQueueMessage message)
        {
            if (message is null)
            {
                throw new ProducingChannelIsNullException(ReservedErrorCode.SystemError, "Producing channel is null.");
            }

            ValidateTopicAndQueue(message.Queue);
        }

        private void ValidateProducingChannel<T>(GeneralQueueMessage<T> message) where T : class
        {
            if (message is null)
            {
                throw new ProducingChannelIsNullException(ReservedErrorCode.SystemError, "Producing channel is null.");
            }

            ValidateTopicAndQueue(message.Queue);
        }

        private void ValidateTopicAndQueue(GeneralQueue queue)
        {
            if (queue is null)
            {
                throw new ProducingChannelIsNullException(ReservedErrorCode.SystemError, "Producing channel is null.");
            }

            if (string.IsNullOrEmpty(queue.Exchange) && string.IsNullOrEmpty(queue.Name))
            {
                throw new ProducingChannelIsNullException(ReservedErrorCode.SystemError, "Topic or Queue name is missing.");
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
