using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.PostgreSql.Migrator;
using CrystalSharp.Sql.Common.Extensions;
using CrystalSharp.Sql.Common.Migrator;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalSharp.PostgreSql.Stores
{
    public static class PostgreSqlEventStoreSetup
    {
        private static Assembly Assembly { get; } = typeof(PostgreSqlEventStoreSetup).GetTypeInfo().Assembly;

        private static IEnumerable<SqlScriptMeta> GetSqlScripts(params string[] scripts)
        {
            string startsWith = "CrystalSharp.PostgreSql.Scripts";

            return Assembly.GetEmbeddedSqlScripts(startsWith, scripts);
        }

        public static void Run(IPostgreSqlDatabaseMigrator postgreSqlDatabaseMigrator, string connectionString, string schema = "")
        {
            IEnumerable<SqlScriptMeta> sqlScripts = GetSqlScripts("Script0001", "Script0002");
            IDictionary<string, string> variables = new Dictionary<string, string>
            {
                {"DbSchema", SelectSchema(schema)},
                {"DbEventStoreTable", EventStoreSettings.EventStoreTable},
                {"DbSnapshotStoreTable", SnapshotStoreSettings.SnapshotTable}
            };
            
            postgreSqlDatabaseMigrator.MigrateDatabaseUsingScripts(connectionString, sqlScripts, variables);
        }

        private static string SelectSchema(string schema)
        {
            return string.IsNullOrEmpty(schema) ? nameof(DbSchema.Public).ToLower() : schema;
        }
    }
}
