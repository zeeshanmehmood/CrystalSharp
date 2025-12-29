using CrystalSharp.EntityFrameworkCore.Common.Stores;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.PostgreSql.Stores
{
    public class PostgreSqlReadModelStore<TDbContext, TKey>(TDbContext dbContext) : ReadModelStore<TDbContext, TKey>(dbContext), IReadModelStore<TKey>
        where TDbContext : DbContext
    {
        //
    }
}
