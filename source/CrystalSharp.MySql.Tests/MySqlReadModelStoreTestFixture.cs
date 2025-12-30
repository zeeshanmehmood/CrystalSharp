using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.MySql.Tests
{
    public class MySqlReadModelStoreTestFixture : IntegrationTestBase, IDisposable
    {
        public IReadModelStore<int> ReadModelStore { get; private set; }

        public MySqlReadModelStoreTestFixture()
        {
            ConfigureMySqlReadModelStore();

            ReadModelStore = GetService<IReadModelStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
