using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.MsSql.ReadModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MsSql.Interceptors
{
    public class ProductValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                (string sampleProductName, decimal sampleProductPrice) = VirtualShopReadModel.GetSampleProduct();
                (string testProductName, decimal testProductPrice) = VirtualShopReadModel.GetTestProduct();
                var entries = eventData.Context.ChangeTracker.Entries<VirtualShopReadModel>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Product.ToUpper().IsEqual(sampleProductName));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.Change(testProductName, testProductPrice);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
