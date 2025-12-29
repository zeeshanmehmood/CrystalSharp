using CrystalSharp.Sql.Common.Migrator;
using DbUp;
using DbUp.Builder;
using System.Collections.Generic;

namespace CrystalSharp.PostgreSql.Migrator
{
    public class PostgreSqlDatabaseMigrator : SqlDatabaseMigrator, IPostgreSqlDatabaseMigrator
    {
        protected override UpgradeEngineBuilder For(SupportedDatabases supportedDatabases, string connectionString, IDictionary<string, string> variables)
        {
            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            return supportedDatabases.PostgresqlDatabase(connectionString).WithVariables(variables);
        }
    }
}
