using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate;
using CrystalSharp.Tests.Common.MsSql.Aggregates.InvoiceAggregate;
using CrystalSharp.Tests.Common.MsSql.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.MsSql.Infrastructure
{
    public class MsSqlAppDbContext : DbContext, IMsSqlDataContext
    {
        public DbSet<Currency> Currency { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<LineItem> LineItem { get; set; }

        public MsSqlAppDbContext()
        {
            //
        }

        public MsSqlAppDbContext(DbContextOptions<MsSqlAppDbContext> options)
            : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new CurrencyEntityConfiguration());
            builder.ApplyConfiguration(new InvoiceEntityConfiguration());
        }
    }
}
