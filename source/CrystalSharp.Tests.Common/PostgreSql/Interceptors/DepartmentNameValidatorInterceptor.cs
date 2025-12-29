using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.PostgreSql.Interceptors
{
    public class DepartmentNameValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                string sampleDepartmentName = Department.GetSampleDepartmentName();
                string testDepartmentName = Department.GetTestDepartmentName();
                var entries = eventData.Context.ChangeTracker.Entries<Department>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Name.ToUpper().IsEqual(sampleDepartmentName));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.ChangeName(testDepartmentName);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
