using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.Sql.Common.Extensions;
using CrystalSharp.Sql.Common.Migrator;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalSharp.MsSql.Stores
{
    public static class MsSqlEventStoreSetup
    {
        private static Assembly Assembly { get; } = typeof(MsSqlEventStoreSetup).GetTypeInfo().Assembly;

        private static IEnumerable<SqlScriptMeta> GetSqlScripts(params string[] scripts)
        {
            string startsWith = "CrystalSharp.MsSql.Scripts";

            return Assembly.GetEmbeddedSqlScripts(startsWith, scripts);
        }

        public static void Run(IMsSqlDatabaseMigrator msSqlDatabaseMigrator, string connectionString, string schema = "")
        {
            IEnumerable<SqlScriptMeta> sqlScripts = GetSqlScripts("Script0001", "Script0002");
            IDictionary<string, string> variables = new Dictionary<string, string>
            {
                {"DbSchema", SelectSchema(schema)},
                {"DbEventStoreTable", EventStoreSettings.EventStoreTable},
                {"DbSnapshotStoreTable", SnapshotStoreSettings.SnapshotTable}
            };
            
            msSqlDatabaseMigrator.MigrateDatabaseUsingScripts(connectionString, sqlScripts, variables);
        }

        private static string SelectSchema(string schema)
        {
            return schema.IsValidString() ? schema : nameof(DbSchema.Dbo).ToLower();
        }
    }
}
