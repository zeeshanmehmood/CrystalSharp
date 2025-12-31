using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Oracle.Interceptors
{
    public class SaleOrderCodeValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string sampleSaleOrderCode = SaleOrder.GetSampleSaleOrderCode();
                string testSaleOrderCode = SaleOrder.GetTestSaleOrderCode();
                var entries = eventData.Context.ChangeTracker.Entries<SaleOrder>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Code.ToUpper().IsEqual(sampleSaleOrderCode));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeCode(testSaleOrderCode);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
