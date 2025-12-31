using CrystalSharp.EntityFrameworkCore.Common.Settings;

namespace CrystalSharp.Oracle.Settings
{
    public class OracleSettings(string connectionString) : EntityFrameworkCoreDbSettings(connectionString)
    {
        //
    }
}
