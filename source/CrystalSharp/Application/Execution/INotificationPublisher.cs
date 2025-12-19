using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Execution
{
    public interface INotificationPublisher
    {
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
    }
}
