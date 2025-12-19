using CrystalSharp.Envoy;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Execution
{
    public class NotificationPublisher(IEnvoy envoy) : INotificationPublisher
    {
        private readonly IEnvoy _envoy = envoy;

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            try
            {
                await _envoy.Publish(notification, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }
    }
}
