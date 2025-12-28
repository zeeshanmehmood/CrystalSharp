using CrystalSharp.EntityFrameworkCore.Common.Stores;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.MsSql.Stores
{
    public class MsSqlReadModelStore<TDbContext, TKey>(TDbContext dbContext) : ReadModelStore<TDbContext, TKey>(dbContext), IReadModelStore<TKey>
        where TDbContext : DbContext
    {
        //
    }
}
