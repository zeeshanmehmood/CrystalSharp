using CrystalSharp.Sql.Common.Migrator;
using DbUp;
using DbUp.Builder;
using System.Collections.Generic;

namespace CrystalSharp.MySql.Migrator
{
    public class MySqlDatabaseMigrator : SqlDatabaseMigrator, IMySqlDatabaseMigrator
    {
        protected override UpgradeEngineBuilder For(
            SupportedDatabases supportedDatabases,
            string connectionString,
            IDictionary<string, string> variables)
        {
            EnsureDatabase.For.MySqlDatabase(connectionString);

            return supportedDatabases.MySqlDatabase(connectionString).WithVariables(variables);
        }
    }
}
