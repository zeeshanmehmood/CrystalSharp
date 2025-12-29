using CrystalSharp.Infrastructure;
using CrystalSharp.PostgreSql.Migrator;
using CrystalSharp.Sagas;
using CrystalSharp.Sql.Common.Extensions;
using CrystalSharp.Sql.Common.Migrator;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalSharp.PostgreSql.Stores
{
    public static class PostgreSqlSagaStoreSetup
    {
        private static Assembly Assembly { get; } = typeof(PostgreSqlSagaStoreSetup).GetTypeInfo().Assembly;

        private static IEnumerable<SqlScriptMeta> GetSqlScripts(params string[] scripts)
        {
            string startsWith = "CrystalSharp.PostgreSql.Scripts";

            return Assembly.GetEmbeddedSqlScripts(startsWith, scripts);
        }

        public static void Run(IPostgreSqlDatabaseMigrator postgreSqlDatabaseMigrator, string connectionString, string schema = "")
        {
            IEnumerable<SqlScriptMeta> sqlScripts = GetSqlScripts("Script0003");
            IDictionary<string, string> variables = new Dictionary<string, string>
            {
                {"DbSchema", SelectSchema(schema)},
                {"DbSagaStoreTable", SagaStoreSettings.SagaStoreTable}
            };
            
            postgreSqlDatabaseMigrator.MigrateDatabaseUsingScripts(connectionString, sqlScripts, variables);
        }

        private static string SelectSchema(string schema)
        {
            return string.IsNullOrEmpty(schema) ? nameof(DbSchema.Public).ToLower() : schema;
        }
    }
}
