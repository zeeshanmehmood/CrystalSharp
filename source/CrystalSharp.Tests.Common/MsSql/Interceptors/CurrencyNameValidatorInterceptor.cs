using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MsSql.Interceptors
{
    public class CurrencyNameValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string sampleCurrencyName = Currency.GetSampleCurrencyName();
                string testCurrencyName = Currency.GetTestCurrencyName();
                var entries = eventData.Context.ChangeTracker.Entries<Currency>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Name.ToUpper().IsEqual(sampleCurrencyName));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeName(testCurrencyName);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
