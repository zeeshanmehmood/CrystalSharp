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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CrystalSharp.Envoy.Contracts;

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
            IEnumerable<NotificationHandlerAssistant> notificationHandlerAssistants = serviceProvider.GetServices<INotificationHandler<TNotification>>()
                .Select(static h => new NotificationHandlerAssistant((notification, token) => h.Handle((TNotification)notification, token)));

            return notificationPublisher(notificationHandlerAssistants, notificationMessage, cancellationToken);
        }
    }
}
