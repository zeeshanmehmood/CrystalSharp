using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.MsSql.Tests
{
    public class MsSqlReadModelStoreTestFixture : IntegrationTestBase, IDisposable
    {
        public IReadModelStore<int> ReadModelStore { get; private set; }

        public MsSqlReadModelStoreTestFixture()
        {
            ConfigureMsSqlReadModelStore();

            ReadModelStore = GetService<IReadModelStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
