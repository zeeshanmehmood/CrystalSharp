using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Tests.Common;
using System;

namespace CrystalSharp.Messaging.RabbitMq.Tests
{
    public class RabbitMqTestFixture : IntegrationTestBase, IDisposable
    {
        public IMessageBroker MessageBroker { get; private set; }

        public RabbitMqTestFixture()
        {
            ConfigureRabbitMq();

            MessageBroker = GetService<IMessageBroker>();
        }

        public void Dispose()
        {
            //
        }
    }
}
