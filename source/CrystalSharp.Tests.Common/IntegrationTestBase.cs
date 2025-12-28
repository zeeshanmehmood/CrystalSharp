using CrystalSharp.EventStores.KurrentDb.Extensions;
using CrystalSharp.Messaging.AzureServiceBus.Configuration;
using CrystalSharp.Messaging.AzureServiceBus.Extensions;
using CrystalSharp.Messaging.RabbitMq.Configuration;
using CrystalSharp.Messaging.RabbitMq.Extensions;
using CrystalSharp.MsSql.Extensions;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.MsSql.Settings;
using CrystalSharp.MsSql.Stores;
using CrystalSharp.Tests.Common.Envoy.Requests;
using CrystalSharp.Tests.Common.MsSql.Infrastructure;
using CrystalSharp.Tests.Common.MsSql.Interceptors;
using Microsoft.EntityFrameworkCore;
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

        protected void ConfigureMsSql()
        {
            Resolver = ConfigureServicesWithMsSql(_configurationRoot);
            MsSqlAppDbContext dbContext = GetService<MsSqlAppDbContext>();

            dbContext.Database.Migrate();
        }

        protected void ConfigureMsSqlEventStore()
        {
            Resolver = ConfigureServicesWithMsSqlEventStore(_configurationRoot);
        }

        protected void ConfigureMsSqlReadModelStore()
        {
            Resolver = ConfigureServicesWithMsSqlReadModelStore(_configurationRoot);
            MsSqlAppDbReadModelStoreContext readModelStoreDbContext = GetService<MsSqlAppDbReadModelStoreContext>();

            readModelStoreDbContext.Database.Migrate();
        }

        protected void ConfigureAzureServiceBus()
        {
            Resolver = ConfigureServicesWithAzureServiceBus(_configurationRoot);
        }

        protected void ConfigureRabbitMq()
        {
            Resolver = ConfigureServicesWithRabbitMq(_configurationRoot);
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

        protected IResolver ConfigureServicesWithMsSql(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("MsSqlDbContext");
            MsSqlSettings msSqlSettings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IMsSqlDataContext>(s => s.GetRequiredService<MsSqlAppDbContext>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMsSql<MsSqlAppDbContext>(
                msSqlSettings,
                typeof(CurrencyNameValidatorInterceptor),
                typeof(InvoiceCodeValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMsSqlEventStore(IConfigurationRoot configurationRoot)
        {
            string eventStoreConnectionString = configurationRoot.GetConnectionString("MsSqlEventStoreDb");
            MsSqlSettings msSqlEventStoreSettings = new(eventStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMsSqlEventStoreDb<int>(msSqlEventStoreSettings).CreateResolver();
            IMsSqlDatabaseMigrator msSqlDatabaseMigrator = resolver.Resolve<IMsSqlDatabaseMigrator>();

            MsSqlEventStoreSetup.Run(msSqlDatabaseMigrator, msSqlEventStoreSettings.ConnectionString);

            return resolver;
        }

        protected IResolver ConfigureServicesWithMsSqlReadModelStore(IConfigurationRoot configurationRoot)
        {
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("MsSqlReadModelStoreDbContext");
            MsSqlSettings msSqlReadModelStoreSettings = new(readModelStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMsSqlReadModelStore<MsSqlAppDbReadModelStoreContext, int>(
                msSqlReadModelStoreSettings,
                typeof(ProductValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithAzureServiceBus(IConfigurationRoot configurationRoot)
        {
            string configurationSection = "AppConfiguration:AzureServiceBusConfiguration:";
            string connectionString = configurationRoot.GetSection($"{configurationSection}ConnectionString").Value;
            AzureServiceBusSettings settings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddAzureServiceBus(settings).CreateResolver();
        }

        protected IResolver ConfigureServicesWithRabbitMq(IConfigurationRoot configurationRoot)
        {
            string configurationSection = "AppConfiguration:RabbitMqConfiguration:";
            string host = configurationRoot.GetSection($"{configurationSection}Host").Value;
            int port = int.Parse(configurationRoot.GetSection($"{configurationSection}Port").Value);
            string username = configurationRoot.GetSection($"{configurationSection}Username").Value;
            string password = configurationRoot.GetSection($"{configurationSection}Password").Value;
            string clientProvidedName = configurationRoot.GetSection($"{configurationSection}ClientProvidedName").Value;
            string virtualHost = configurationRoot.GetSection($"{configurationSection}VirtualHost").Value;
            RabbitMqSettings settings = new(host, port, username, password, clientProvidedName, virtualHost);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddRabbitMq(settings).CreateResolver();
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
