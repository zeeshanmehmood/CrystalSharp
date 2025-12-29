using CrystalSharp.EntityFrameworkCore.Common.Settings;

namespace CrystalSharp.PostgreSql.Settings
{
    public class PostgreSqlSettings(string connectionString) : EntityFrameworkCoreDbSettings(connectionString)
    {
        public string Schema { get; set; }
    }
}
