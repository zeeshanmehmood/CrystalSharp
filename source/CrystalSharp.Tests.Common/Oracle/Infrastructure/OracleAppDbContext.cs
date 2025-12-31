using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate;
using CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate;
using CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure
{
    public class OracleAppDbContext : DbContext, IOracleDataContext
    {
        public DbSet<Employee> Employee { get; set; }
        public DbSet<SaleOrder> SaleOrder { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }

        public OracleAppDbContext()
        {
            //
        }

        public OracleAppDbContext(DbContextOptions<OracleAppDbContext> options)
            : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new EmployeeEntityConfiguration());
            builder.ApplyConfiguration(new SaleOrderEntityConfiguration());
            builder.ApplyConfiguration(new OrderDetailEntityConfiguration());
        }
    }
}
