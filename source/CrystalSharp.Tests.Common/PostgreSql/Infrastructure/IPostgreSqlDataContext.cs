using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.ReceiptAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.PostgreSql.Infrastructure
{
    public interface IPostgreSqlDataContext
    {
        DbSet<Department> Department { get; set; }
        DbSet<Receipt> Receipt { get; set; }
        DbSet<InventoryItem> InventoryItem { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
