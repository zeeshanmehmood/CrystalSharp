using CrystalSharp.EntityFrameworkCore.Common.Stores;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Oracle.Stores
{
    public class OracleReadModelStore<TDbContext, TKey>(TDbContext dbContext) : ReadModelStore<TDbContext, TKey>(dbContext), IReadModelStore<TKey>
        where TDbContext : DbContext
    {
        //
    }
}
