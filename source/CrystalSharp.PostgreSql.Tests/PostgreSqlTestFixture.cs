using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.PostgreSql.Infrastructure;
using System;

namespace CrystalSharp.PostgreSql.Tests
{
    public class PostgreSqlTestFixture : IntegrationTestBase, IDisposable
    {
        public IPostgreSqlDataContext DataContext { get; private set; }

        public PostgreSqlTestFixture()
        {
            ConfigurePostgreSql();

            DataContext = GetService<IPostgreSqlDataContext>();
        }

        public void Dispose()
        {
            //
        }
    }
}
