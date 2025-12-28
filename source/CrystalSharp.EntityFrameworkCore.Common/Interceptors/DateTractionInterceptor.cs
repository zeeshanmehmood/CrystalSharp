using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.EntityFrameworkCore.Common.Interceptors
{
    public sealed class DateTractionInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                DateTraction(eventData.Context);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
        }

        private void DateTraction(DbContext dbContext)
        {
            var modifiedItems = dbContext.ChangeTracker
                .Entries<IModifiedOnEntity>()
                .Where(entity => entity.State == EntityState.Modified);

            var newItems = dbContext.ChangeTracker
                .Entries<ICreatedAtEntity>()
                .Where(entity => entity.State == EntityState.Added);

            foreach (var item in modifiedItems)
            {
                item.Entity.SetModifiedOn(SystemDate.UtcNow);
            }

            foreach (var item in newItems)
            {
                item.Entity.SetCreatedAt(SystemDate.UtcNow);
            }
        }
    }
}
