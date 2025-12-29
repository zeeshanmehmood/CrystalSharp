using CrystalSharp.Tests.Common.PostgreSql.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.PostgreSql.Infrastructure
{
    public class PostgreSqlAppDbReadModelStoreContext : DbContext
    {
        public DbSet<DepartmentReadModel> DepartmentReadModel { get; set; }

        public PostgreSqlAppDbReadModelStoreContext()
        {
            //
        }

        public PostgreSqlAppDbReadModelStoreContext(DbContextOptions<PostgreSqlAppDbReadModelStoreContext> options)
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
