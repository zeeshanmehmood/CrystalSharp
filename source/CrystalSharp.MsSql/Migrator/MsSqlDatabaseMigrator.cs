using CrystalSharp.Sql.Common.Migrator;
using DbUp;
using DbUp.Builder;
using System.Collections.Generic;

namespace CrystalSharp.MsSql.Migrator
{
    public class MsSqlDatabaseMigrator : SqlDatabaseMigrator, IMsSqlDatabaseMigrator
    {
        protected override UpgradeEngineBuilder For(SupportedDatabases supportedDatabases, string connectionString, IDictionary<string, string> variables)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);

            return supportedDatabases.SqlDatabase(connectionString).WithVariables(variables);
        }
    }
}
