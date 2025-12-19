using CrystalSharp.Envoy.Contracts;
using CrystalSharp.Envoy.Decorators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Envoy
{
    public class EnvoyImpl(IServiceProvider serviceProvider) : IEnvoy
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private static readonly ConcurrentDictionary<Type, RequestHandlerDecorator> _requestHandlerStore = new();
        private static readonly ConcurrentDictionary<Type, NotificationHandlerDecorator> _notificationHandlerStore = new();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            RequestHandlerDecorator<TResponse> handler = (RequestHandlerDecorator<TResponse>)GetRequestHandler(request.GetType(), typeof(TResponse));

            return await handler.Handle(request, _serviceProvider, cancellationToken).ConfigureAwait(false);
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotificationMessage
        {
            ArgumentNullException.ThrowIfNull(notification);

            NotificationHandlerDecorator handler = GetNotificationHandler(notification.GetType());

            await handler.Handle(notification, NotificationPublisher, _serviceProvider, cancellationToken).ConfigureAwait(false);
        }

        private async Task NotificationPublisher(IEnumerable<NotificationHandlerAssistant> notificationHandlerAssistants,
            INotificationMessage notification,
            CancellationToken cancellationToken = default)
        {
            if (notificationHandlerAssistants is not null
                && notificationHandlerAssistants.Any())
            {
                foreach (NotificationHandlerAssistant notificationHandlerAssistant in notificationHandlerAssistants)
                {
                    await notificationHandlerAssistant.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private RequestHandlerDecorator GetRequestHandler(Type requestType, Type responseType)
        {
            static RequestHandlerDecorator GetRequestHandlerDecorator(Type request, Type response)
            {
                Type[] types = [request, response];
                Type constructedType = typeof(RequestHandlerDecorator<,>).MakeGenericType(types);
                object concreteType = Activator.CreateInstance(constructedType) ?? throw new InvalidOperationException($"Could not create decorator for type {request}");

                return (RequestHandlerDecorator)concreteType;
            }

            return _requestHandlerStore.GetOrAdd(requestType, GetRequestHandlerDecorator(requestType, responseType));
        }

        private NotificationHandlerDecorator GetNotificationHandler(Type notificationType)
        {
            static NotificationHandlerDecorator GetNotificationHandlerDecorator(Type notification)
            {
                Type constructedType = typeof(NotificationHandlerDecorator<>).MakeGenericType(notification);
                object concreteType = Activator.CreateInstance(constructedType) ?? throw new InvalidOperationException($"Could not create decorator for type {notification}");

                return (NotificationHandlerDecorator)concreteType;
            }

            return _notificationHandlerStore.GetOrAdd(notificationType, GetNotificationHandlerDecorator(notificationType));
        }
    }
}
