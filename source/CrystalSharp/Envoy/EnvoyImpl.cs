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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Envoy.Contracts;
using CrystalSharp.Envoy.Decorators;

namespace CrystalSharp.Envoy
{
    public class EnvoyImpl : IEnvoy
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<Type, RequestHandlerDecorator> _requestHandlerStore = new();
        private static readonly ConcurrentDictionary<Type, NotificationHandlerDecorator> _notificationHandlerStore = new();

        public EnvoyImpl(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            RequestHandlerDecorator<TResponse> handler = (RequestHandlerDecorator<TResponse>)GetRequestHandler(request.GetType(), typeof(TResponse));

            return await handler.Handle(request, _serviceProvider, cancellationToken).ConfigureAwait(false);
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotificationMessage
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));

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
                Type[] types = new[] { request, response };
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
