using System;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalSharp.Sql.Common.Migrator
{
    public interface ISqlDatabaseMigrator
    {
        void MigrateDatabaseUsingEmbeddedScripts(Assembly assembly, string connectionString, IDictionary<string, string> variables);
        void MigrateDatabaseUsingEmbeddedScripts(Assembly assembly, string connectionString, Func<string, bool> filter, IDictionary<string, string> variables);
        void MigrateDatabaseUsingScripts(string connectionString, IEnumerable<SqlScriptMeta> scripts, IDictionary<string, string> variables);
    }
}
