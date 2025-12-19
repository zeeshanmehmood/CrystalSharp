using CrystalSharp.Domain.Infrastructure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Domain.EventDispatching
{
    public interface IEventDispatcher
    {
        Task Dispatch(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default);
    }
}
