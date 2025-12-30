using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MySql.Interceptors
{
    public class SupplierNameValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string sampleSupplierName = Supplier.GetSampleSupplierName();
                string testSupplierName = Supplier.GetTestSupplierName();
                var entries = eventData.Context.ChangeTracker.Entries<Supplier>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Name.ToUpper().IsEqual(sampleSupplierName));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeName(testSupplierName);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
