using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MsSql.Infrastructure;
using System;

namespace CrystalSharp.MsSql.Tests
{
    public class MsSqlTestFixture : IntegrationTestBase, IDisposable
    {
        public IMsSqlDataContext DataContext { get; private set; }

        public MsSqlTestFixture()
        {
            ConfigureMsSql();

            DataContext = GetService<IMsSqlDataContext>();
        }

        public void Dispose()
        {
            //
        }
    }
}
