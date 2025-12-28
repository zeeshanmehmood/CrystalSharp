using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate;
using CrystalSharp.Tests.Common.MsSql.Aggregates.InvoiceAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.MsSql.Infrastructure
{
    public interface IMsSqlDataContext
    {
        DbSet<Currency> Currency { get; set; }
        DbSet<Invoice> Invoice { get; set; }
        DbSet<LineItem> LineItem { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
