using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.MySql.Migrator;
using CrystalSharp.Sql.Common.Extensions;
using CrystalSharp.Sql.Common.Migrator;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalSharp.MySql.Stores
{
    public static class MySqlEventStoreSetup
    {
        private static Assembly Assembly { get; } = typeof(MySqlEventStoreSetup).GetTypeInfo().Assembly;

        private static IEnumerable<SqlScriptMeta> GetSqlScripts(params string[] scripts)
        {
            string startsWith = "CrystalSharp.MySql.Scripts";

            return Assembly.GetEmbeddedSqlScripts(startsWith, scripts);
        }

        public static void Run(IMySqlDatabaseMigrator mySqlDatabaseMigrator, string connectionString)
        {
            IEnumerable<SqlScriptMeta> sqlScripts = GetSqlScripts("Script0001", "Script0002");
            IDictionary<string, string> variables = new Dictionary<string, string>
            {
                {"DbEventStoreTable", EventStoreSettings.EventStoreTable},
                {"DbSnapshotStoreTable", SnapshotStoreSettings.SnapshotTable}
            };
            
            mySqlDatabaseMigrator.MigrateDatabaseUsingScripts(connectionString, sqlScripts, variables);
        }
    }
}
