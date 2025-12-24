using System.Threading.RateLimiting;

namespace CrystalSharp.Messaging.RabbitMq.Configuration
{
    public class RabbitMqChannelConfiguration(bool publisherConfirmationsEnabled,
        bool publisherConfirmationTrackingEnabled,
        RateLimiter outstandingPublisherConfirmationsRateLimiter = null,
        ushort? consumerDispatchConcurrency = 1)
    {
        public readonly bool PublisherConfirmationsEnabled = publisherConfirmationsEnabled;
        public readonly bool PublisherConfirmationTrackingEnabled = publisherConfirmationTrackingEnabled;
        public readonly RateLimiter OutstandingPublisherConfirmationsRateLimiter = outstandingPublisherConfirmationsRateLimiter;
        public readonly ushort? ConsumerDispatchConcurrency = consumerDispatchConcurrency;
    }
}
