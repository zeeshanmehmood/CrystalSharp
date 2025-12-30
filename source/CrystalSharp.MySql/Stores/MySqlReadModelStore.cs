using CrystalSharp.EntityFrameworkCore.Common.Stores;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.MySql.Stores
{
    public class MySqlReadModelStore<TDbContext, TKey>(TDbContext dbContext) : ReadModelStore<TDbContext, TKey>(dbContext), IReadModelStore<TKey>
        where TDbContext : DbContext
    {
        //
    }
}
