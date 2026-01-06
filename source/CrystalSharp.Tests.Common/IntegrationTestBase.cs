using CrystalSharp.EntityFrameworkCore.Common.Interceptors;
using CrystalSharp.EventStores.KurrentDb.Extensions;
using CrystalSharp.Messaging.AzureServiceBus.Configuration;
using CrystalSharp.Messaging.AzureServiceBus.Extensions;
using CrystalSharp.Messaging.RabbitMq.Configuration;
using CrystalSharp.Messaging.RabbitMq.Extensions;
using CrystalSharp.MongoDb.Extensions;
using CrystalSharp.MongoDb.Settings;
using CrystalSharp.MsSql.Extensions;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.MsSql.Settings;
using CrystalSharp.MsSql.Stores;
using CrystalSharp.MySql.Extensions;
using CrystalSharp.MySql.Migrator;
using CrystalSharp.MySql.Settings;
using CrystalSharp.MySql.Stores;
using CrystalSharp.Oracle.Extensions;
using CrystalSharp.Oracle.Settings;
using CrystalSharp.PostgreSql.Extensions;
using CrystalSharp.PostgreSql.Migrator;
using CrystalSharp.PostgreSql.Settings;
using CrystalSharp.PostgreSql.Stores;
using CrystalSharp.Tests.Common.Envoy.Requests;
using CrystalSharp.Tests.Common.MsSql.Infrastructure;
using CrystalSharp.Tests.Common.MsSql.Interceptors;
using CrystalSharp.Tests.Common.MySql.Infrastructure;
using CrystalSharp.Tests.Common.MySql.Interceptors;
using CrystalSharp.Tests.Common.Oracle.Infrastructure;
using CrystalSharp.Tests.Common.Oracle.Interceptors;
using CrystalSharp.Tests.Common.PostgreSql.Infrastructure;
using CrystalSharp.Tests.Common.PostgreSql.Interceptors;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        protected void ConfigureOracle()
        {
            Resolver = ConfigureServicesWithOracle(_configurationRoot);
            OracleAppDbContext dbContext = GetService<OracleAppDbContext>();

            dbContext.Database.Migrate();
        }

        protected void ConfigureOracleReadModelStore()
        {
            Resolver = ConfigureServicesWithOracleReadModelStore(_configurationRoot);
            OracleAppDbReadModelStoreContext readModelStoreDbContext = GetService<OracleAppDbReadModelStoreContext>();

            readModelStoreDbContext.Database.Migrate();
        }

        protected void ConfigurePostgreSql()
        {
            Resolver = ConfigureServicesWithPostgreSql(_configurationRoot);
            PostgreSqlAppDbContext dbContext = GetService<PostgreSqlAppDbContext>();

            dbContext.Database.Migrate();
        }

        protected void ConfigurePostgreSqlEventStore()
        {
            Resolver = ConfigureServicesWithPostgreSqlEventStore(_configurationRoot);
        }

        protected void ConfigurePostgreSqlReadModelStore()
        {
            Resolver = ConfigureServicesWithPostgreSqlReadModelStore(_configurationRoot);
            PostgreSqlAppDbReadModelStoreContext readModelStoreDbContext = GetService<PostgreSqlAppDbReadModelStoreContext>();

            readModelStoreDbContext.Database.Migrate();
        }

        protected void ConfigureMySql()
        {
            Resolver = ConfigureServicesWithMySql(_configurationRoot);
            MySqlAppDbContext dbContext = GetService<MySqlAppDbContext>();

            dbContext.Database.Migrate();
        }

        protected void ConfigureMySqlEventStore()
        {
            Resolver = ConfigureServicesWithMySqlEventStore(_configurationRoot);
        }

        protected void ConfigureMySqlReadModelStore()
        {
            Resolver = ConfigureServicesWithMySqlReadModelStore(_configurationRoot);
            MySqlAppDbReadModelStoreContext readModelStoreDbContext = GetService<MySqlAppDbReadModelStoreContext>();

            readModelStoreDbContext.Database.Migrate();
        }

        protected void ConfigureMongoDb(string database, string eventStoreDatabase, string readModelStoreDatabase)
        {
            Resolver = ConfigureServicesWithMongoDb(_configurationRoot, database, eventStoreDatabase, readModelStoreDatabase);
        }

        protected void ConfigureMsSqlSagas()
        {
            Resolver = ConfigureServicesWithMsSqlSagas(_configurationRoot);
        }

        protected void ConfigurePostgreSqlSagas()
        {
            Resolver = ConfigureServicesWithPostgreSqlSagas(_configurationRoot);
        }

        protected void ConfigureMySqlSagas()
        {
            Resolver = ConfigureServicesWithMySqlSagas(_configurationRoot);
        }

        protected void ConfigureMongoDbSagas(string databaseToUse)
        {
            Resolver = ConfigureServicesWithMongoDbSagas(_configurationRoot, databaseToUse);
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

        protected IResolver ConfigureServicesWithOracle(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("OracleDbContext");
            OracleSettings oracleSettings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IOracleDataContext>(s => s.GetRequiredService<OracleAppDbContext>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddOracle<OracleAppDbContext>(
                oracleSettings,
                typeof(EmployeeNameValidatorInterceptor),
                typeof(SaleOrderCodeValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithOracleReadModelStore(IConfigurationRoot configurationRoot)
        {
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("OracleReadModelStoreDbContext"); ;
            OracleSettings oracleReadModelStoreSettings = new(readModelStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddOracleReadModelStore<OracleAppDbReadModelStoreContext, int>(
                oracleReadModelStoreSettings,
                typeof(CustomerValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithPostgreSql(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("PostgreSqlDbContext");
            PostgreSqlSettings postgreSqlSettings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IPostgreSqlDataContext>(s => s.GetRequiredService<PostgreSqlAppDbContext>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddPostgreSql<PostgreSqlAppDbContext>(
                postgreSqlSettings,
                typeof(DepartmentNameValidatorInterceptor),
                typeof(ReceiptCodeValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithPostgreSqlEventStore(IConfigurationRoot configurationRoot)
        {
            string eventStoreConnectionString = configurationRoot.GetConnectionString("PostgreSqlEventStoreDb");
            PostgreSqlSettings postgreSqlEventStoreSettings = new(eventStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddPostgreSqlEventStoreDb<int>(postgreSqlEventStoreSettings).CreateResolver();
            IPostgreSqlDatabaseMigrator postgreSqlDatabaseMigrator = resolver.Resolve<IPostgreSqlDatabaseMigrator>();

            PostgreSqlEventStoreSetup.Run(postgreSqlDatabaseMigrator, postgreSqlEventStoreSettings.ConnectionString);

            return resolver;
        }

        protected IResolver ConfigureServicesWithPostgreSqlReadModelStore(IConfigurationRoot configurationRoot)
        {
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("PostgreSqlReadModelStoreDbContext");
            PostgreSqlSettings postgreSqlReadModelStoreSettings = new(readModelStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddPostgreSqlReadModelStore<PostgreSqlAppDbReadModelStoreContext, int>(
                postgreSqlReadModelStoreSettings,
                typeof(DepartmentValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMySql(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("MySqlDbContext");
            MySqlSettings mySqlSettings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IMySqlDataContext>(s => s.GetRequiredService<MySqlAppDbContext>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMySql<MySqlAppDbContext>(
                mySqlSettings,
                typeof(SupplierNameValidatorInterceptor),
                typeof(PurchaseOrderCodeValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMySqlEventStore(IConfigurationRoot configurationRoot)
        {
            string eventStoreConnectionString = configurationRoot.GetConnectionString("MySqlEventStoreDb");
            MySqlSettings mySqlEventStoreSettings = new(eventStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMySqlEventStoreDb<int>(mySqlEventStoreSettings).CreateResolver();
            IMySqlDatabaseMigrator mySqlDatabaseMigrator = resolver.Resolve<IMySqlDatabaseMigrator>();

            MySqlEventStoreSetup.Run(mySqlDatabaseMigrator, mySqlEventStoreSettings.ConnectionString);

            return resolver;
        }

        protected IResolver ConfigureServicesWithMySqlReadModelStore(IConfigurationRoot configurationRoot)
        {
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("MySqlReadModelStoreDbContext");
            MySqlSettings mySqlReadModelStoreSettings = new(readModelStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMySqlReadModelStore<MySqlAppDbReadModelStoreContext, int>(
                mySqlReadModelStoreSettings,
                typeof(SupplierValidatorInterceptor))
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMongoDb(
            IConfigurationRoot configurationRoot,
            string database,
            string eventStoreDatabase,
            string readModelStoreDatabase)
        {
            string connectionString = configurationRoot.GetConnectionString("MongoDbConnectionString");
            string eventStoreConnectionString = configurationRoot.GetConnectionString("MongoDbEventStoreConnectionString");
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("MongoDbReadModelStoreConnectionString");
            MongoDbSettings mongoDbSettings = new(connectionString, database);
            MongoDbSettings mongoDbEventStoreSettings = new(eventStoreConnectionString, eventStoreDatabase);
            MongoDbSettings mongoDbReadModelStoreSettings = new(readModelStoreConnectionString, readModelStoreDatabase);

            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddMongoDb(mongoDbSettings)
                .AddMongoDbEventStoreDb<string>(mongoDbEventStoreSettings)
                .AddMongoDbReadModelStore(mongoDbReadModelStoreSettings)
                .CreateResolver();
        }

        protected IResolver ConfigureServicesWithMsSqlSagas(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("MsSqlSagasConnectionString");
            MsSqlSettings settings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            RegisterDateTractionAndEventDispatcherInterceptors(crystalSharpAdapter);
            crystalSharpAdapter.ServiceCollection.AddDbContext<InMemoryDbContext>((sp, options) =>
            {
                List<IInterceptor> interceptors = [sp.GetRequiredService<DateTractionInterceptor>(), sp.GetRequiredService<DispatchDomainEventsInterceptor>()];

                options.UseInMemoryDatabase("crystalsharp-mssql-data-inmemory").AddInterceptors(interceptors);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IInMemoryDataContext>(s => s.GetRequiredService<InMemoryDbContext>());

            IResolver resolver = crystalSharpAdapter.AddMsSqlSagaStore(settings, typeof(PlaceOrderTransaction)).CreateResolver();
            IMsSqlDatabaseMigrator msSqlDatabaseMigrator = resolver.Resolve<IMsSqlDatabaseMigrator>();

            MsSqlSagaStoreSetup.Run(msSqlDatabaseMigrator, settings.ConnectionString);

            return resolver;
        }

        protected IResolver ConfigureServicesWithPostgreSqlSagas(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("PostgreSqlSagasConnectionString");
            PostgreSqlSettings settings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            RegisterDateTractionAndEventDispatcherInterceptors(crystalSharpAdapter);
            crystalSharpAdapter.ServiceCollection.AddDbContext<InMemoryDbContext>((sp, options) =>
            {
                List<IInterceptor> interceptors = [sp.GetRequiredService<DateTractionInterceptor>(), sp.GetRequiredService<DispatchDomainEventsInterceptor>()];

                options.UseInMemoryDatabase("crystalsharp-postgresql-data-inmemory").AddInterceptors(interceptors);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IInMemoryDataContext>(s => s.GetRequiredService<InMemoryDbContext>());

            IResolver resolver = crystalSharpAdapter.AddPostgreSqlSagaStore(settings, typeof(PlaceOrderTransaction)).CreateResolver();
            IPostgreSqlDatabaseMigrator postgreSqlDatabaseMigrator = resolver.Resolve<IPostgreSqlDatabaseMigrator>();

            PostgreSqlSagaStoreSetup.Run(postgreSqlDatabaseMigrator, settings.ConnectionString);

            return resolver;
        }

        protected IResolver ConfigureServicesWithMySqlSagas(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("MySqlSagasConnectionString");
            MySqlSettings settings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            RegisterDateTractionAndEventDispatcherInterceptors(crystalSharpAdapter);
            crystalSharpAdapter.ServiceCollection.AddDbContext<InMemoryDbContext>((sp, options) =>
            {
                List<IInterceptor> interceptors = [sp.GetRequiredService<DateTractionInterceptor>(), sp.GetRequiredService<DispatchDomainEventsInterceptor>()];

                options.UseInMemoryDatabase("crystalsharp-mysql-data-inmemory").AddInterceptors(interceptors);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IInMemoryDataContext>(s => s.GetRequiredService<InMemoryDbContext>());

            IResolver resolver = crystalSharpAdapter.AddMySqlSagaStore(settings, typeof(PlaceOrderTransaction)).CreateResolver();
            IMySqlDatabaseMigrator mySqlDatabaseMigrator = resolver.Resolve<IMySqlDatabaseMigrator>();

            MySqlSagaStoreSetup.Run(mySqlDatabaseMigrator, settings.ConnectionString);

            return resolver;
        }

        protected IResolver ConfigureServicesWithMongoDbSagas(IConfigurationRoot configurationRoot, string databaseToUse)
        {
            string connectionString = configurationRoot.GetConnectionString("MongoDbSagasConnectionString");
            MongoDbSettings settings = new(connectionString, databaseToUse);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            RegisterDateTractionAndEventDispatcherInterceptors(crystalSharpAdapter);
            crystalSharpAdapter.ServiceCollection.AddDbContext<InMemoryDbContext>((sp, options) =>
            {
                List<IInterceptor> interceptors = [sp.GetRequiredService<DateTractionInterceptor>(), sp.GetRequiredService<DispatchDomainEventsInterceptor>()];

                options.UseInMemoryDatabase("crystalsharp-mongodb-data-inmemory").AddInterceptors(interceptors);
            });
            crystalSharpAdapter.ServiceCollection.AddScoped<IInMemoryDataContext>(s => s.GetRequiredService<InMemoryDbContext>());

            return crystalSharpAdapter.AddMongoDbSagaStore(settings, typeof(PlaceOrderTransaction)).CreateResolver();
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

        private void RegisterDateTractionAndEventDispatcherInterceptors(ICrystalSharpAdapter crystalSharpAdapter)
        {
            ServiceDescriptor dateTractionInterceptorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(DateTractionInterceptor));
            ServiceDescriptor dispatchDomainEventsInterceptorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(DispatchDomainEventsInterceptor));

            if (dateTractionInterceptorDescriptor is null)
            {
                crystalSharpAdapter.ServiceCollection.AddSingleton<DateTractionInterceptor>();
            }

            if (dispatchDomainEventsInterceptorDescriptor is null)
            {
                crystalSharpAdapter.ServiceCollection.AddSingleton<DispatchDomainEventsInterceptor>();
            }
        }

        private ICrystalSharpAdapter ConfigureCrystalSharpAdapter(IServiceCollection services)
        {
            return CrystalSharpAdapter.New(services).AddCqrs(typeof(CreateProductRequest));
        }
    }
}
