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

namespace CrystalSharp.Messaging.RabbitMq.Configuration
{
    public class RabbitMqSettings
    {
        public string Host { get; }
        public int Port { get;}
        public string Username { get;}
        public string Password { get; }
        public string ClientProvidedName { get; }
        public string VirtualHost { get; }
        public bool AutomaticRecoveryEnabled { get; }
        public bool TopologyRecoveryEnabled { get; }
        public TimeSpan RequestedConnectionTimeout { get; }
        public TimeSpan RequestedHeartbeat { get; }
        public bool DispatchConsumersAsync { get; }
        public int InitialConnectionRetries { get; }
        public int InitialConnectionRetryTimeoutMilliseconds { get; }

        public RabbitMqSettings(string host,
            int port,
            string username = "",
            string password = "",
            string clientProvidedName = "",
            string virualHost = "",
            bool automaticRecoveryEnabled = true,
            bool topologyRecoveryEnabled = true,
            double requestedConnectionTimeoutInSeconds = 60,
            double requestedHeartbeatInSeconds = 60,
            bool dispatchConsumersAsync = true,
            int initialConnectionRetries = 5,
            int initialConnectionRetryTimeoutMilliseconds = 200)
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            ClientProvidedName = clientProvidedName;
            VirtualHost = virualHost;
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            TopologyRecoveryEnabled = topologyRecoveryEnabled;
            RequestedConnectionTimeout = TimeSpan.FromSeconds(requestedConnectionTimeoutInSeconds);
            RequestedHeartbeat = TimeSpan.FromSeconds(requestedHeartbeatInSeconds);
            DispatchConsumersAsync = dispatchConsumersAsync;
            InitialConnectionRetries = initialConnectionRetries;
            InitialConnectionRetryTimeoutMilliseconds = initialConnectionRetryTimeoutMilliseconds;
        }
    }
}
