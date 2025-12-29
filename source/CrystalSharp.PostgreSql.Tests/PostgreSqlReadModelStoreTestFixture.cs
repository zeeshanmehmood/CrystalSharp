using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.PostgreSql.Tests
{
    public class PostgreSqlReadModelStoreTestFixture : IntegrationTestBase, IDisposable
    {
        public IReadModelStore<int> ReadModelStore { get; private set; }

        public PostgreSqlReadModelStoreTestFixture()
        {
            ConfigurePostgreSqlReadModelStore();

            ReadModelStore = GetService<IReadModelStore<int>>();
        }

        public void Dispose()
        {
            //
        }
    }
}
