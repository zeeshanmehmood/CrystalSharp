using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate;
using CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure
{
    public interface IOracleDataContext
    {
        DbSet<Employee> Employee { get; set; }
        DbSet<SaleOrder> SaleOrder { get; set; }
        DbSet<OrderDetail> OrderDetail { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
