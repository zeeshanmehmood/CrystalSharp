using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Domain.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.EntityFrameworkCore.Common.Interceptors
{
    public sealed class DispatchDomainEventsInterceptor(IEventDispatcher eventDispatcher) : SaveChangesInterceptor
    {
        private readonly IEventDispatcher _eventDispatcher = eventDispatcher;

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                List<EntityEntry> afftectedEntries = [.. eventData.Context.ChangeTracker.Entries().Where(x => x.Entity is IHasSecondaryId)];
                List<List<IDomainEvent>> eventsToDispatch = [];

                foreach (EntityEntry entry in afftectedEntries)
                {
                    if (entry.Entity is IHasDomainEvents entity && entity.EventsCount() > 0)
                    {
                        IReadOnlyList<IDomainEvent> domainEvents = entity.UncommittedEvents();

                        eventsToDispatch.Add([.. domainEvents.Select(x => x)]);
                        entity.MarkEventsAsCommitted();
                    }
                }

                if (eventsToDispatch.HasAny())
                {
                    foreach (List<IDomainEvent> events in eventsToDispatch)
                    {
                        await _eventDispatcher.Dispatch(events.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return result;
        }
    }
}
