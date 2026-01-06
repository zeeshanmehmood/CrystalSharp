using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using System;

namespace CrystalSharp.Sagas.Tests
{
    public class MySqlSagaTestFixture : IntegrationTestBase, IDisposable
    {
        public ISagaTransactionExecutor SagaTransactionExecutor { get; private set; }
        public IInMemoryDataContext DataContext { get; private set; }
        public ISagaStore SagaStore { get; private set; }

        public MySqlSagaTestFixture()
        {
            ConfigureMySqlSagas();

            SagaTransactionExecutor = GetService<ISagaTransactionExecutor>();
            DataContext = GetService<IInMemoryDataContext>();
            SagaStore = GetService<ISagaStore>();
        }

        public void Dispose()
        {
            //
        }
    }
}
