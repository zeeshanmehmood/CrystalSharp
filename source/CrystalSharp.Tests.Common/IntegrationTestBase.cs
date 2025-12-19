using CrystalSharp.EventStores.KurrentDb.Extensions;
using CrystalSharp.Tests.Common.Envoy.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace CrystalSharp.Tests.Common
{
    public class IntegrationTestBase
    {
        protected IResolver Resolver;
        private readonly IConfigurationRoot _configurationRoot;

        protected IntegrationTestBase()
        {
            _configurationRoot = GetConfigurationRoot();
        }

        protected void ConfigureKurrentDb()
        {
            Resolver = ConfigureServicesWithKurrentDb(_configurationRoot);
        }

        protected IConfigurationRoot GetConfigurationRoot()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return configurationRoot;
        }

        protected IResolver ConfigureServicesWithKurrentDb(IConfigurationRoot configurationRoot)
        {
            string configurationSection = "AppConfiguration:EventStoreConfiguration:";
            string host = configurationRoot.GetSection($"{configurationSection}Host").Value;
            int port = int.Parse(configurationRoot.GetSection($"{configurationSection}Port").Value);
            string username = configurationRoot.GetSection($"{configurationSection}Username").Value;
            string password = configurationRoot.GetSection($"{configurationSection}Password").Value;
            string eventStoreConnectionString = $"esdb://{host}:{port}?tls=false";
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddKurrentDbEventStore<int>(eventStoreConnectionString).CreateResolver();
        }

        protected T GetService<T>()
        {
            return Resolver.Resolve<T>();
        }

        private ICrystalSharpAdapter ConfigureCrystalSharpAdapter(IServiceCollection services)
        {
            return CrystalSharpAdapter.New(services).AddCqrs(typeof(CreateProductRequest));
        }
    }
}
