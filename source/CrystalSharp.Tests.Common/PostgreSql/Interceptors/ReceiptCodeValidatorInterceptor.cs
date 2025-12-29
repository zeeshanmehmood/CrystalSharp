using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.ReceiptAggregate;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.PostgreSql.Interceptors
{
    public class ReceiptCodeValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string sampleReceiptCode = Receipt.GetSampleReceiptCode();
                string testReceiptCode = Receipt.GetTestReceiptCode();
                var entries = eventData.Context.ChangeTracker.Entries<Receipt>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Code.ToUpper().IsEqual(sampleReceiptCode));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeCode(testReceiptCode);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
