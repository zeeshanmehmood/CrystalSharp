using CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration;
using CrystalSharp.Tests.Common.Oracle.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure
{
    public class OracleAppDbReadModelStoreContext : DbContext
    {
        public DbSet<CustomerReadModel> CustomerReadModel { get; set; }

        public OracleAppDbReadModelStoreContext()
        {
            //
        }

        public OracleAppDbReadModelStoreContext(DbContextOptions<OracleAppDbReadModelStoreContext> options)
            : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new CustomerReadModelEntityConfiguration());
        }
    }
}
