using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Envoy;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Domain.EventDispatching
{
    public class TransactionalDomainEventDispatcher(IEnvoy envoy) : IEventDispatcher
    {
        private readonly IEnvoy _envoy = envoy;

        public async Task Dispatch(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            foreach (IDomainEvent @event in events)
            {
                await _envoy.Publish(@event, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
