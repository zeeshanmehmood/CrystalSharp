using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.PostgreSql.ReadModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.PostgreSql.Interceptors
{
    public class DepartmentValidatorInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData is not null && eventData.Context is not null)
            {
                (string sampleDepartmentName, string sampleDepartmentCode) = DepartmentReadModel.GetSampleDepartment();
                (string testDepartmentName, string testDepartmentCode) = DepartmentReadModel.GetTestDepartment();
                var entries = eventData.Context.ChangeTracker.Entries<DepartmentReadModel>();

                if (entries.HasAny())
                {
                    var entry = entries.FirstOrDefault(x => x.Entity.Name.ToUpper().IsEqual(sampleDepartmentName) && x.Entity.Code.ToUpper().IsEqual(sampleDepartmentCode));

                    if (entry is not null && entry.Entity is not null)
                    {
                        entry.Entity.Change(testDepartmentName, testDepartmentCode);
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
