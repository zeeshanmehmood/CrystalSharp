using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Messaging.Distributed
{
    public interface IMessageBroker
    {
        Task PublishObject<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default) where T : class;
        Task PublishJson(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default);
        Task PublishString(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default);
        Task SendObject<T>(IDistributedMessage<T> distributedMessage, CancellationToken cancellationToken = default) where T : class;
        Task SendJson(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default);
        Task SendString(IDistributedMessage distributedMessage, CancellationToken cancellationToken = default);
        Task StartConsuming(IConsumer consumer, CancellationToken cancellationToken = default);
        Task StopConsuming(CancellationToken cancellationToken = default);
        Task Disconnect(CancellationToken cancellationToken = default);
    }
}
