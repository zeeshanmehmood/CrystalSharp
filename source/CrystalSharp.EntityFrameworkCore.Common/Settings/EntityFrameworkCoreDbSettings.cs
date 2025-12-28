using CrystalSharp.Infrastructure;

namespace CrystalSharp.EntityFrameworkCore.Common.Settings
{
    public abstract class EntityFrameworkCoreDbSettings(string connectionString) : DbSettings(connectionString)
    {
        public bool LazyLoading { get; set; } = false;
    }
}
