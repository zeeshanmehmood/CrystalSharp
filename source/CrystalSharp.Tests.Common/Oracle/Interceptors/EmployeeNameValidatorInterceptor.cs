using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Oracle.Interceptors
{
    public class EmployeeNameValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string sampleEmployeeName = Employee.GetSampleEmployeeName();
                string testEmployeeName = Employee.GetTestEmployeeName();
                var entries = eventData.Context.ChangeTracker.Entries<Employee>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Name.ToUpper().IsEqual(sampleEmployeeName));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeName(testEmployeeName);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
