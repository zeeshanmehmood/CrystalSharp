using CrystalSharp.Tests.Common.MsSql.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.MsSql.Infrastructure
{
    public class MsSqlAppDbReadModelStoreContext : DbContext
    {
        public DbSet<VirtualShopReadModel> VirtualShopReadModel { get; set; }

        public MsSqlAppDbReadModelStoreContext()
        {
            //
        }

        public MsSqlAppDbReadModelStoreContext(DbContextOptions<MsSqlAppDbReadModelStoreContext> options)
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
