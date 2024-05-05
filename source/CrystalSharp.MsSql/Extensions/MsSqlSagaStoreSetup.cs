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

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Infrastructure;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.Sagas;
using CrystalSharp.Sql.Common.Extensions;
using CrystalSharp.Sql.Common.Migrator;

namespace CrystalSharp.MsSql.Extensions
{
    public static class MsSqlSagaStoreSetup
    {
        private static Assembly Assembly { get; } = typeof(MsSqlSagaStoreSetup).GetTypeInfo().Assembly;

        private static IEnumerable<SqlScriptMeta> GetSqlScripts(params string[] scripts)
        {
            string startsWith = "CrystalSharp.MsSql.Scripts";

            return Assembly.GetEmbeddedSqlScripts(startsWith, scripts);
        }

        public static Task Run(IMsSqlDatabaseMigrator msSqlDatabaseMigrator,
            string connectionString,
            string schema = "",
            CancellationToken cancellationToken = default)
        {
            IEnumerable<SqlScriptMeta> sqlScripts = GetSqlScripts("Script0003");
            IDictionary<string, string> variables = new Dictionary<string, string>
            {
                {"DbSchema", SelectSchema(schema)},
                {"DbSagaStoreTable", SagaStoreSettings.SagaStoreTable}
            };

            return msSqlDatabaseMigrator.MigrateDatabaseUsingScripts(connectionString, sqlScripts, variables, cancellationToken);
        }

        private static string SelectSchema(string schema)
        {
            return string.IsNullOrEmpty(schema) ? nameof(DbSchema.Dbo).ToLower() : schema;
        }
    }
}
