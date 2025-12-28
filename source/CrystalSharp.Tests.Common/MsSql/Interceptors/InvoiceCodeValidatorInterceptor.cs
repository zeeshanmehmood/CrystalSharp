using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.MsSql.Aggregates.InvoiceAggregate;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MsSql.Interceptors
{
    public class InvoiceCodeValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string sampleInvoiceCode = Invoice.GetSampleInvoiceCode();
                string testInvoiceCode = Invoice.GetTestInvoiceCode();
                var entries = eventData.Context.ChangeTracker.Entries<Invoice>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Code.ToUpper().IsEqual(sampleInvoiceCode));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeCode(testInvoiceCode);
                    }
                }
            }
            
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
