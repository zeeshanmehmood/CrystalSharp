using CrystalSharp.Envoy.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Envoy
{
    public record NotificationHandlerAssistant(Func<INotificationMessage, CancellationToken, Task> HandlerCallback);
}
