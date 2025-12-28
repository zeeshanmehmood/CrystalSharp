using CrystalSharp.Sql.Common.Exceptions;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrystalSharp.Sql.Common.Migrator
{
    public abstract class SqlDatabaseMigrator : ISqlDatabaseMigrator
    {
        protected abstract UpgradeEngineBuilder For(SupportedDatabases supportedDatabases, string connectionString, IDictionary<string, string> variables);

        public virtual void MigrateDatabaseUsingEmbeddedScripts(Assembly assembly, string connectionString, IDictionary<string, string> variables)
        {
            Migrate(assembly, connectionString, null, variables);
        }

        public virtual void MigrateDatabaseUsingEmbeddedScripts(Assembly assembly, string connectionString, Func<string, bool> filter, IDictionary<string, string> variables)
        {
            Migrate(assembly, connectionString, filter, variables);
        }

        public virtual void MigrateDatabaseUsingScripts(string connectionString, IEnumerable<SqlScriptMeta> scripts, IDictionary<string, string> variables)
        {
            MigrateWithScripts(scripts, connectionString, variables);
        }

        protected virtual void Migrate(Assembly assembly, string connectionString, Func<string, bool> filter, IDictionary<string, string> variables)
        {
            UpgradeEngine upgradeEngine = For(DeployChanges.To, connectionString, variables)
                .WithScriptsEmbeddedInAssembly(assembly, filter, new SqlScriptOptions { ScriptType = ScriptType.RunOnce })
                .WithTransaction()
                .Build();
            
            Upgrade(upgradeEngine);
        }

        protected virtual void MigrateWithScripts(IEnumerable<SqlScriptMeta> scripts, string connectionString, IDictionary<string, string> variables)
        {
            IEnumerable<SqlScript> dbScripts = scripts.Select(script => new SqlScript(script.Name, script.Content));
            UpgradeEngine upgradeEngine = For(DeployChanges.To, connectionString, variables)
                .WithScripts(dbScripts)
                .WithTransaction()
                .Build();
            
            Upgrade(upgradeEngine);
        }

        protected virtual void Upgrade(UpgradeEngine upgradeEngine)
        {
            IList<string> scripts = [.. upgradeEngine.GetScriptsToExecute().Select(script => script.Name)];
            DatabaseUpgradeResult upgradeResult = upgradeEngine.PerformUpgrade();

            if (!upgradeResult.Successful)
            {
                throw new SqlMigrationException(scripts, upgradeResult.Error.Message, upgradeResult.Error);
            }
        }
    }
}
