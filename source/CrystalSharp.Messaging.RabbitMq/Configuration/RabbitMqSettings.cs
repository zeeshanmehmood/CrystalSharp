using System;

namespace CrystalSharp.Messaging.RabbitMq.Configuration
{
    public class RabbitMqSettings(string host,
        int port,
        string username = "",
        string password = "",
        string clientProvidedName = "",
        string virtualHost = "",
        RabbitMqSslOptions sslOption = null,
        bool automaticRecoveryEnabled = true,
        bool topologyRecoveryEnabled = true,
        long requestedConnectionTimeout = 60,
        long requestedHeartbeat = 60)
    {
        public string Host { get; } = host;
        public int Port { get; } = port;
        public string Username { get; } = username;
        public string Password { get; } = password;
        public string ClientProvidedName { get; } = clientProvidedName;
        public string VirtualHost { get; } = virtualHost;
        public RabbitMqSslOptions SslOption { get; } = sslOption;
        public bool AutomaticRecoveryEnabled { get; } = automaticRecoveryEnabled;
        public bool TopologyRecoveryEnabled { get; } = topologyRecoveryEnabled;
        public TimeSpan RequestedConnectionTimeout { get; } = TimeSpan.FromSeconds(requestedConnectionTimeout);
        public TimeSpan RequestedHeartbeat { get; } = TimeSpan.FromSeconds(requestedHeartbeat);
    }
}
