using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.ReceiptAggregate;
using CrystalSharp.Tests.Common.PostgreSql.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.PostgreSql.Infrastructure
{
    public class PostgreSqlAppDbContext : DbContext, IPostgreSqlDataContext
    {
        public DbSet<Department> Department { get; set; }
        public DbSet<Receipt> Receipt { get; set; }
        public DbSet<InventoryItem> InventoryItem { get; set; }

        public PostgreSqlAppDbContext()
        {
            //
        }

        public PostgreSqlAppDbContext(DbContextOptions<PostgreSqlAppDbContext> options)
            : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new DepartmentEntityConfiguration());
            builder.ApplyConfiguration(new ReceiptEntityConfiguration());
        }
    }
}
