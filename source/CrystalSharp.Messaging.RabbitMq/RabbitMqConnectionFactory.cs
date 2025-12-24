using CrystalSharp.Common.Settings;
using CrystalSharp.Messaging.Distributed.Exceptions;
using CrystalSharp.Messaging.RabbitMq.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Messaging.RabbitMq
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        public async Task<IConnection> CreateConnection(RabbitMqSettings settings, CancellationToken cancellationToken = default)
        {
            IConnection connection;

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
                RequestedHeartbeat = settings.RequestedHeartbeat
            };

            if (settings.SslOption is not null)
            {
                connectionFactory.Ssl = new(settings.SslOption.ServerName, settings.SslOption.CertificatePath, settings.SslOption.Enabled);
            }

            try
            {
                connection = await connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (BrokerUnreachableException exception)
            {
                string errorMessage = $"Could not establish an initial connection.";

                throw new InitialConnectionException(ReservedErrorCode.SystemError, errorMessage, exception);
            }
            

            return connection;
        }

        public async Task<IConnection> CreateConnection(Uri connectionUri, CancellationToken cancellationToken = default)
        {
            ConnectionFactory connectionFactory = new() { Uri = connectionUri };
            IConnection connection;

            try
            {
                connection = await connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (BrokerUnreachableException exception)
            {
                string errorMessage = $"Could not establish an initial connection.";

                throw new InitialConnectionException(ReservedErrorCode.SystemError, errorMessage, exception);
            }

            return connection;
        }
    }
}
