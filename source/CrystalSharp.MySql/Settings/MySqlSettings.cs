using CrystalSharp.EntityFrameworkCore.Common.Settings;

namespace CrystalSharp.MySql.Settings
{
    public class MySqlSettings(string connectionString) : EntityFrameworkCoreDbSettings(connectionString)
    {
        //
    }
}
