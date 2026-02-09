using CrystalSharp.Messaging.AzureServiceBus.Configuration;
using CrystalSharp.Messaging.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CrystalSharp.Messaging.AzureServiceBus.Extensions
{
    public static class CrystalSharpAdapterAzureServiceBusExtensions
    {
        public static ICrystalSharpAdapter AddAzureServiceBus(this ICrystalSharpAdapter crystalSharpAdapter, AzureServiceBusSettings settings)
        {
            crystalSharpAdapter.Register<AzureServiceBusSettings>(s => settings, ServiceLifetime.Scoped);
            crystalSharpAdapter.Register<IMessageBroker, AzureServiceBusMessageBroker>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }
    }
}
