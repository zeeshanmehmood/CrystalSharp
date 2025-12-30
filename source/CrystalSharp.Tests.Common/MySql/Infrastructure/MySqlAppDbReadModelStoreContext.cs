using CrystalSharp.Tests.Common.MySql.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.MySql.Infrastructure
{
    public class MySqlAppDbReadModelStoreContext : DbContext
    {
        public DbSet<SupplierReadModel> SupplierReadModel { get; set; }

        public MySqlAppDbReadModelStoreContext()
        {
            //
        }

        public MySqlAppDbReadModelStoreContext(DbContextOptions<MySqlAppDbReadModelStoreContext> options)
            : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
