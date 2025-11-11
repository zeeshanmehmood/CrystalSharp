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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure;

namespace CrystalSharp.EntityFrameworkCore.Common.Database
{
    public abstract class EntityFrameworkCoreDbContext
    {
        private readonly IEventDispatcher _eventDispatcher;

        protected EntityFrameworkCoreDbContext(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task<int> SaveChanges<TDbContext>(TDbContext dbContext, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
        {
            List<EntityEntry> afftectedEntries = dbContext.ChangeTracker.Entries().Where(x => x.Entity is IHasSecondaryId).ToList();
            List<List<IDomainEvent>> eventsToDispatch = new();

            DateTraction(dbContext);

            foreach (EntityEntry entry in afftectedEntries)
            {
                if (entry.Entity is IHasDomainEvents entity && entity.EventsCount() > 0)
                {
                    IReadOnlyList<IDomainEvent> domainEvents = entity.UncommittedEvents();

                    eventsToDispatch.Add(domainEvents.Select(x => x).ToList());
                    entity.MarkEventsAsCommitted();
                }
            }

            await dbContext.SaveChangesAsync(true, cancellationToken).ConfigureAwait(false);

            int affected = dbContext.ChangeTracker.Entries().Where(x => x.Entity is IHasSecondaryId).Count();

            if (affected > 0 && eventsToDispatch.HasAny())
            {
                foreach (List<IDomainEvent> events in eventsToDispatch)
                {
                    await _eventDispatcher.Dispatch(events.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                }
            }

            return affected;
        }

        private void DateTraction<TDbContext>(TDbContext dbContext) where TDbContext : DbContext
        {
            var modifiedItems = dbContext.ChangeTracker
                .Entries<IModifiedOnTrackedEntity>()
                .Where(entity => entity.State == EntityState.Modified);

            var newItems = dbContext.ChangeTracker
                .Entries<ICreatedOnTrackedEntity>()
                .Where(entity => entity.State == EntityState.Added);

            foreach (var item in modifiedItems)
            {
                item.Entity.SetModifiedOn(SystemDate.UtcNow);
            }

            foreach (var item in newItems)
            {
                item.Entity.SetCreatedOn(SystemDate.UtcNow);
            }
        }
    }
}
