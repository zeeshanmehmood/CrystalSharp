using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.Oracle.ReadModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Oracle.Interceptors
{
    public class CustomerValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                (string sampleCustomerName, string sampleCustomerCode) = CustomerReadModel.GetSampleCustomer();
                (string testCustomerName, string testCustomerCode) = CustomerReadModel.GetTestCustomer();
                var entries = eventData.Context.ChangeTracker.Entries<CustomerReadModel>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Name.ToUpper().IsEqual(sampleCustomerName) && x.Entity.Code.ToUpper().IsEqual(sampleCustomerCode));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.Change(testCustomerName, testCustomerCode);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
