using CrystalSharp.Messaging.RabbitMq.Configuration;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Messaging.RabbitMq
{
    public interface IRabbitMqConnectionFactory
    {
        Task<IConnection> CreateConnection(RabbitMqSettings settings, CancellationToken cancellationToken = default);
        Task<IConnection> CreateConnection(Uri connectionUri, CancellationToken cancellationToken = default);
    }
}
