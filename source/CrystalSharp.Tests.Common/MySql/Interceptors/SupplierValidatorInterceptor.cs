using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.MySql.ReadModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MySql.Interceptors
{
    public class SupplierValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                (string sampleSupplierName, string sampleSupplierCode) = SupplierReadModel.GetSampleSupplier();
                (string testSupplierName, string testSupplierCode) = SupplierReadModel.GetTestSupplier();
                var entries = eventData.Context.ChangeTracker.Entries<SupplierReadModel>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Name.ToUpper().IsEqual(sampleSupplierName) && x.Entity.Code.ToUpper().IsEqual(sampleSupplierCode));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.Change(testSupplierName, testSupplierCode);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
