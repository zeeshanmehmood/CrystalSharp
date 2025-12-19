using CrystalSharp.Envoy.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Envoy.Decorators
{
    public abstract class NotificationHandlerDecorator
    {
        public abstract Task Handle(INotificationMessage notificationMessage,
            Func<IEnumerable<NotificationHandlerAssistant>, INotificationMessage, CancellationToken, Task> notificationPublisher,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default);
    }

    public class NotificationHandlerDecorator<TNotification> : NotificationHandlerDecorator
        where TNotification : INotificationMessage
    {
        public override Task Handle(INotificationMessage notificationMessage,
            Func<IEnumerable<NotificationHandlerAssistant>, INotificationMessage, CancellationToken, Task> notificationPublisher,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<NotificationHandlerAssistant> notificationHandlerAssistants = serviceProvider
                .GetServices<INotificationHandler<TNotification>>()
                .Select(static h =>
                    new NotificationHandlerAssistant((notification, token) =>
                    h.Handle((TNotification)notification, token)));

            return notificationPublisher(notificationHandlerAssistants, notificationMessage, cancellationToken);
        }
    }
}
