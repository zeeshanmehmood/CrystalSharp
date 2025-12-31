using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Oracle.Infrastructure;
using System;

namespace CrystalSharp.Oracle.Tests
{
    public class OracleTestFixture : IntegrationTestBase, IDisposable
    {
        public IOracleDataContext DataContext { get; private set; }

        public OracleTestFixture()
        {
            ConfigureOracle();

            DataContext = GetService<IOracleDataContext>();
        }

        public void Dispose()
        {
            //
        }
    }
}
