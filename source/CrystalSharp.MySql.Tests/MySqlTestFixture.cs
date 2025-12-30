using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MySql.Infrastructure;
using System;

namespace CrystalSharp.MySql.Tests
{
    public class MySqlTestFixture : IntegrationTestBase, IDisposable
    {
        public IMySqlDataContext DataContext { get; private set; }

        public MySqlTestFixture()
        {
            ConfigureMySql();

            DataContext = GetService<IMySqlDataContext>();
        }

        public void Dispose()
        {
            //
        }
    }
}
