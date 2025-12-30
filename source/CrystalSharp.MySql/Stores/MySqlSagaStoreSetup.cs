using CrystalSharp.MySql.Migrator;
using CrystalSharp.Sagas;
using CrystalSharp.Sql.Common.Extensions;
using CrystalSharp.Sql.Common.Migrator;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalSharp.MySql.Stores
{
    public static class MySqlSagaStoreSetup
    {
        private static Assembly Assembly { get; } = typeof(MySqlSagaStoreSetup).GetTypeInfo().Assembly;

        private static IEnumerable<SqlScriptMeta> GetSqlScripts(params string[] scripts)
        {
            string startsWith = "CrystalSharp.MySql.Scripts";

            return Assembly.GetEmbeddedSqlScripts(startsWith, scripts);
        }

        public static void Run(IMySqlDatabaseMigrator mySqlDatabaseMigrator, string connectionString)
        {
            IEnumerable<SqlScriptMeta> sqlScripts = GetSqlScripts("Script0003");
            IDictionary<string, string> variables = new Dictionary<string, string>
            {
                {"DbSagaStoreTable", SagaStoreSettings.SagaStoreTable}
            };
            
            mySqlDatabaseMigrator.MigrateDatabaseUsingScripts(connectionString, sqlScripts, variables);
        }
    }
}
