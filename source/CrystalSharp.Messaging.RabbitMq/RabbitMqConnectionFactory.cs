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
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Messaging.Distributed.Exceptions;
using CrystalSharp.Messaging.RabbitMq.Configuration;

namespace CrystalSharp.Messaging.RabbitMq
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        public IConnection CreateConnection(RabbitMqSettings settings)
        {
            if (settings is null)
            {
                return null;
            }

            ConnectionFactory connectionFactory = new()
            {
                HostName = settings.Host,
                Port = settings.Port,
                UserName = settings.Username,
                Password = settings.Password,
                ClientProvidedName = settings.ClientProvidedName,
                VirtualHost = settings.VirtualHost,
                AutomaticRecoveryEnabled = settings.AutomaticRecoveryEnabled,
                TopologyRecoveryEnabled = settings.TopologyRecoveryEnabled,
                RequestedConnectionTimeout = settings.RequestedConnectionTimeout,
                RequestedHeartbeat = settings.RequestedHeartbeat,
                DispatchConsumersAsync = settings.DispatchConsumersAsync
            };

            return (string.IsNullOrEmpty(settings.ClientProvidedName))
                ?
                CreateUnnamedConnection(settings, connectionFactory)
                :
                CreateNamedConnection(settings, connectionFactory);
        }

        public AsyncEventingBasicConsumer CreateConsumer(IModel channel)
        {
            return new AsyncEventingBasicConsumer(channel);
        }

        private IConnection CreateUnnamedConnection(RabbitMqSettings settings, ConnectionFactory connectionFactory)
        {
            return TryToCreateConnection(connectionFactory.CreateConnection,
                settings.InitialConnectionRetries,
                settings.InitialConnectionRetryTimeoutMilliseconds);
        }

        private IConnection CreateNamedConnection(RabbitMqSettings settings, ConnectionFactory connectionFactory)
        {
            return TryToCreateConnection(() => 
                connectionFactory.CreateConnection(settings.ClientProvidedName),
                settings.InitialConnectionRetries,
                settings.InitialConnectionRetryTimeoutMilliseconds);
        }

        private IConnection TryToCreateConnection(Func<IConnection> connection, int numberOfRetries, int timeoutMilliseconds)
        {
            ValidateArguments(numberOfRetries, timeoutMilliseconds);

            int attempts = 0;
            BrokerUnreachableException brokerUnreachableException = null;

            while (attempts < numberOfRetries)
            {
                try
                {
                    if (attempts > 0)
                    {
                        Thread.Sleep(timeoutMilliseconds);
                    }

                    return connection();
                }
                catch (BrokerUnreachableException exception)
                {
                    attempts++;

                    brokerUnreachableException = exception;
                }
            }

            string errorMessage = $"Could not establish an initial connection in {numberOfRetries} retries.";

            throw new InitialConnectionException(ReservedErrorCode.SystemError, errorMessage, brokerUnreachableException);
        }

        private void ValidateArguments(int numberOfRetries, int timeoutMilliseconds)
        {
            if (numberOfRetries < 1)
            {
                throw new ArgumentException("Number of retries should be a positive number.", nameof(numberOfRetries));
            }

            if (timeoutMilliseconds < 1)
            {
                throw new ArgumentException("Initial reconnection timeout should be a positive number.", nameof(timeoutMilliseconds));
            }
        }
    }
}
