// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Support;
using CrystalSharp.Sql.Common.Exceptions;

namespace CrystalSharp.Sql.Common.Migrator
{
    public abstract class SqlDatabaseMigrator : ISqlDatabaseMigrator
    {
        protected abstract UpgradeEngineBuilder For(SupportedDatabases supportedDatabases, string connectionString, IDictionary<string, string> variables);

        public virtual async Task MigrateDatabaseUsingEmbeddedScripts(Assembly assembly,
            string connectionString,
            IDictionary<string, string> variables,
            CancellationToken cancellationToken = default)
        {
            await Migrate(assembly, connectionString, null, variables, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task MigrateDatabaseUsingEmbeddedScripts(Assembly assembly,
            string connectionString,
            Func<string, bool> filter,
            IDictionary<string, string> variables,
            CancellationToken cancellationToken = default)
        {
            await Migrate(assembly, connectionString, filter, variables, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task MigrateDatabaseUsingScripts(string connectionString,
            IEnumerable<SqlScriptMeta> scripts,
            IDictionary<string, string> variables,
            CancellationToken cancellationToken = default)
        {
            await MigrateWithScripts(scripts, connectionString, variables, cancellationToken).ConfigureAwait(false);
        }

        protected virtual Task Migrate(Assembly assembly,
            string connectionString,
            Func<string, bool> filter,
            IDictionary<string, string> variables,
            CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() =>
            {
                UpgradeEngine upgradeEngine = For(DeployChanges.To, connectionString, variables)
                    .WithScriptsEmbeddedInAssembly(assembly, filter, new SqlScriptOptions { ScriptType = ScriptType.RunOnce })
                    .WithTransaction()
                    .Build();

                Upgrade(upgradeEngine);
            },
            TaskCreationOptions.LongRunning);
        }

        protected virtual Task MigrateWithScripts(IEnumerable<SqlScriptMeta> scripts,
            string connectionString,
            IDictionary<string, string> variables,
            CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() =>
            {
                IEnumerable<SqlScript> dbScripts = scripts.Select(script => new SqlScript(script.Name, script.Content));
                UpgradeEngine upgradeEngine = For(DeployChanges.To, connectionString, variables)
                    .WithScripts(dbScripts)
                    .WithTransaction()
                    .Build();

                Upgrade(upgradeEngine);
            },
            TaskCreationOptions.LongRunning);
        }

        protected virtual void Upgrade(UpgradeEngine upgradeEngine)
        {
            IList<string> scripts = upgradeEngine.GetScriptsToExecute().Select(script => script.Name).ToList();
            DatabaseUpgradeResult upgradeResult = upgradeEngine.PerformUpgrade();

            if (!upgradeResult.Successful)
            {
                throw new SqlMigrationException(scripts, upgradeResult.Error.Message, upgradeResult.Error);
            }
        }
    }
}
