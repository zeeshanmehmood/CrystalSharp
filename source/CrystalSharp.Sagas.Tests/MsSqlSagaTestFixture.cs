using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using System;

namespace CrystalSharp.Sagas.Tests
{
    public class MsSqlSagaTestFixture : IntegrationTestBase, IDisposable
    {
        public ISagaTransactionExecutor SagaTransactionExecutor { get; private set; }
        public IInMemoryDataContext DataContext { get; private set; }
        public ISagaStore SagaStore { get; private set; }

        public MsSqlSagaTestFixture()
        {
            ConfigureMsSqlSagas();

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
