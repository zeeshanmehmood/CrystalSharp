using CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate;
using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MySql.Infrastructure
{
    public interface IMySqlDataContext
    {
        DbSet<Supplier> Supplier { get; set; }
        DbSet<PurchaseOrder> PurchaseOrder { get; set; }
        DbSet<OrderItem> OrderItem { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
