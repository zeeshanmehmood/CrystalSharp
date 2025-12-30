using CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate;
using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate;
using CrystalSharp.Tests.Common.MySql.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.MySql.Infrastructure
{
    public class MySqlAppDbContext : DbContext, IMySqlDataContext
    {
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrder { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        public MySqlAppDbContext()
        {
            //
        }

        public MySqlAppDbContext(DbContextOptions<MySqlAppDbContext> options)
            : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new SupplierEntityConfiguration());
            builder.ApplyConfiguration(new PurchaseOrderEntityConfiguration());
        }
    }
}
