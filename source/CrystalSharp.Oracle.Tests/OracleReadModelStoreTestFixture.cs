using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.Oracle.Tests
{
    public class OracleReadModelStoreTestFixture : IntegrationTestBase, IDisposable
    {
        public IReadModelStore<int> ReadModelStore { get; private set; }

        public OracleReadModelStoreTestFixture()
        {
            ConfigureOracleReadModelStore();

            ReadModelStore = GetService<IReadModelStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
