using CrystalSharp.EntityFrameworkCore.Common.Settings;

namespace CrystalSharp.MsSql.Settings
{
    public class MsSqlSettings(string connectionString) : EntityFrameworkCoreDbSettings(connectionString)
    {
        public string Schema { get; set; }
    }
}
