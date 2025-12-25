using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.Messaging.AzureServiceBus.Tests
{
    public class AzureServiceBusTestFixture : IntegrationTestBase, IDisposable
    {
        public IMessageBroker MessageBroker { get; private set; }

        public AzureServiceBusTestFixture()
        {
            ConfigureAzureServiceBus();

            MessageBroker = GetService<IMessageBroker>();
        }

        public void Dispose()
        {
            //
        }
    }
}
