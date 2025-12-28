using CrystalSharp.Infrastructure;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.Sagas;
using CrystalSharp.Sql.Common.Extensions;
using CrystalSharp.Sql.Common.Migrator;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalSharp.MsSql.Stores
{
    public static class MsSqlSagaStoreSetup
    {
        private static Assembly Assembly { get; } = typeof(MsSqlSagaStoreSetup).GetTypeInfo().Assembly;

        private static IEnumerable<SqlScriptMeta> GetSqlScripts(params string[] scripts)
        {
            string startsWith = "CrystalSharp.MsSql.Scripts";

            return Assembly.GetEmbeddedSqlScripts(startsWith, scripts);
        }

        public static void Run(IMsSqlDatabaseMigrator msSqlDatabaseMigrator, string connectionString, string schema = "")
        {
            IEnumerable<SqlScriptMeta> sqlScripts = GetSqlScripts("Script0003");
            IDictionary<string, string> variables = new Dictionary<string, string>
            {
                {"DbSchema", SelectSchema(schema)},
                {"DbSagaStoreTable", SagaStoreSettings.SagaStoreTable}
            };
            
            msSqlDatabaseMigrator.MigrateDatabaseUsingScripts(connectionString, sqlScripts, variables);
        }

        private static string SelectSchema(string schema)
        {
            return string.IsNullOrEmpty(schema) ? nameof(DbSchema.Dbo).ToLower() : schema;
        }
    }
}
