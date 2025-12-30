using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MySql.Interceptors
{
    public class PurchaseOrderCodeValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string samplePurchaseOrderCode = PurchaseOrder.GetSamplePurchaseOrderCode();
                string testPurchaseOrderCode = PurchaseOrder.GetTestPurchaseOrderCode();
                var entries = eventData.Context.ChangeTracker.Entries<PurchaseOrder>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Code.ToUpper().IsEqual(samplePurchaseOrderCode));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeCode(testPurchaseOrderCode);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
