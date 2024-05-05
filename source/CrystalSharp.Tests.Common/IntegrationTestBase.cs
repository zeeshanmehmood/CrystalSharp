// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CrystalSharp.EventStores.EventStoreDb.Extensions;
using CrystalSharp.Messaging.AzureServiceBus.Configuration;
using CrystalSharp.Messaging.AzureServiceBus.Extensions;
using CrystalSharp.Messaging.RabbitMq.Configuration;
using CrystalSharp.Messaging.RabbitMq.Extensions;
using CrystalSharp.MongoDb.Extensions;
using CrystalSharp.MsSql.Database;
using CrystalSharp.MsSql.Extensions;
using CrystalSharp.MsSql.Migrator;
using CrystalSharp.MySql.Database;
using CrystalSharp.MySql.Extensions;
using CrystalSharp.MySql.Migrator;
using CrystalSharp.Oracle.Extensions;
using CrystalSharp.PostgreSql.Database;
using CrystalSharp.PostgreSql.Extensions;
using CrystalSharp.PostgreSql.Migrator;
using CrystalSharp.RavenDb.Extensions;
using CrystalSharp.ReadModelStores.Elasticsearch.Extensions;
using CrystalSharp.Tests.Common.MsSql.Infrastructure;
using CrystalSharp.Tests.Common.MySql.Infrastructure;
using CrystalSharp.Tests.Common.Oracle.Infrastructure;
using CrystalSharp.Tests.Common.PostgreSql.Infrastructure;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;

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

        protected void ConfigureEventStoreDb()
        {
            Resolver = ConfigureServicesWithEventStoreDb(_configurationRoot);
        }

        protected void ConfigureElasticsearch()
        {
            Resolver = ConfigureServicesWithElasticsearch(_configurationRoot);
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

        protected void ConfigureOracle(string version = "")
        {
            Resolver = ConfigureServicesWithOracle(_configurationRoot, version);
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

        protected void ConfigureRavenDb(string database)
        {
            Resolver = ConfigureServicesWithRavenDb(_configurationRoot, database);
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

        protected IResolver ConfigureServicesWithEventStoreDb(IConfigurationRoot configurationRoot)
        {
            string configurationSection = "AppConfiguration:EventStoreConfiguration:";
            string host = configurationRoot.GetSection($"{configurationSection}Host").Value;
            int port = int.Parse(configurationRoot.GetSection($"{configurationSection}Port").Value);
            string username = configurationRoot.GetSection($"{configurationSection}Username").Value;
            string password = configurationRoot.GetSection($"{configurationSection}Password").Value;
            string eventStoreConnectionString = $"esdb://{host}:{port}?tls=false";
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddEventStoreDbEventStore<int>(eventStoreConnectionString).CreateResolver();
        }

        protected IResolver ConfigureServicesWithElasticsearch(IConfigurationRoot configurationRoot)
        {
            string configurationSection = "AppConfiguration:ElasticsearchConfiguration:";
            string host = configurationRoot.GetSection($"{configurationSection}Host").Value;
            int port = int.Parse(configurationRoot.GetSection($"{configurationSection}Port").Value);
            string username = configurationRoot.GetSection($"{configurationSection}Username").Value;
            string password = configurationRoot.GetSection($"{configurationSection}Password").Value;
            int numberOfReplicas = int.Parse(configurationRoot.GetSection($"{configurationSection}NumberOfReplicas").Value);
            int numberOfShards = int.Parse(configurationRoot.GetSection($"{configurationSection}NumberOfShards").Value);
            string elasticsearchConnectionString = $"{host}:{port}";
            ElasticsearchSettings elasticsearchReadModelStoreSettings = new(elasticsearchConnectionString, numberOfReplicas, numberOfShards);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddElasticsearchReadModelStore<Guid>(elasticsearchReadModelStoreSettings).CreateResolver();
        }

        protected IResolver ConfigureServicesWithMsSql(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("MsSqlDbContext");
            MsSqlSettings msSqlSettings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IMsSqlDataContext>(s => s.GetRequiredService<MsSqlAppDbContext>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMsSql<MsSqlAppDbContext>(msSqlSettings).CreateResolver();

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

            MsSqlEventStoreSetup.Run(msSqlDatabaseMigrator, msSqlEventStoreSettings.ConnectionString).Wait();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMsSqlReadModelStore(IConfigurationRoot configurationRoot)
        {
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("MsSqlReadModelStoreDbContext");
            MsSqlSettings msSqlReadModelStoreSettings = new(readModelStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMsSqlReadModelStore<MsSqlAppDbReadModelStoreContext, int>(msSqlReadModelStoreSettings)
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithOracle(IConfigurationRoot configurationRoot, string version = "")
        {
            string connectionString = configurationRoot.GetConnectionString("OracleDbContext");
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("OracleReadModelStoreDbContext");

            OracleSettings oracleSettings = new(connectionString, version);
            OracleSettings oracleReadModelStoreSettings = new(readModelStoreConnectionString, version);

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IOracleDataContext>(s => s.GetRequiredService<OracleAppDbContext>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddOracle<OracleAppDbContext>(oracleSettings)
                .AddOracleReadModelStore<OracleAppDbReadModelStoreContext, int>(oracleReadModelStoreSettings)
                .CreateResolver();
        }

        protected IResolver ConfigureServicesWithPostgreSql(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("PostgreSqlDbContext");
            PostgreSqlSettings postgreSqlSettings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IPostgreSqlDataContext>(s => s.GetRequiredService<PostgreSqlAppDbContext>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddPostgreSql<PostgreSqlAppDbContext>(postgreSqlSettings).CreateResolver();

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

            PostgreSqlEventStoreSetup.Run(postgreSqlDatabaseMigrator, postgreSqlEventStoreSettings.ConnectionString).Wait();

            return resolver;
        }

        protected IResolver ConfigureServicesWithPostgreSqlReadModelStore(IConfigurationRoot configurationRoot)
        {
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("PostgreSqlReadModelStoreDbContext");
            PostgreSqlSettings postgreSqlReadModelStoreSettings = new(readModelStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddPostgreSqlReadModelStore<PostgreSqlAppDbReadModelStoreContext, int>(postgreSqlReadModelStoreSettings)
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
            IResolver resolver = crystalSharpAdapter.AddMySql<MySqlAppDbContext>(mySqlSettings).CreateResolver();

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

            MySqlEventStoreSetup.Run(mySqlDatabaseMigrator, mySqlEventStoreSettings.ConnectionString).Wait();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMySqlReadModelStore(IConfigurationRoot configurationRoot)
        {
            string readModelStoreConnectionString = configurationRoot.GetConnectionString("MySqlReadModelStoreDbContext");
            MySqlSettings mySqlReadModelStoreSettings = new(readModelStoreConnectionString);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMySqlReadModelStore<MySqlAppDbReadModelStoreContext, int>(mySqlReadModelStoreSettings)
                .CreateResolver();

            return resolver;
        }

        protected IResolver ConfigureServicesWithRavenDb(IConfigurationRoot configurationRoot, string database)
        {
            string connectionUrl = configurationRoot.GetConnectionString("RavenDbConnectionUrl");
            RavenDbSettings settings = new(connectionUrl, database);
            IServiceCollection serviceCollection = new ServiceCollection();
            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            return crystalSharpAdapter.AddRavenDb(settings).CreateResolver();
        }

        protected IResolver ConfigureServicesWithMongoDb(IConfigurationRoot configurationRoot,
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

            serviceCollection.AddDbContext<InMemoryDbContext<IMsSqlEntityFrameworkCoreContext>>(s => 
                s.UseInMemoryDatabase("crystalsharp-mssql-data-inmemory"));
            serviceCollection.AddScoped<IMsSqlEntityFrameworkCoreContext, MsSqlEntityFrameworkCoreContext>();
            serviceCollection.AddScoped<IInMemoryDataContext>(s => 
                s.GetRequiredService<InMemoryDbContext<IMsSqlEntityFrameworkCoreContext>>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

            IResolver resolver = crystalSharpAdapter.AddMsSqlSagaStore(settings, typeof(PlaceOrderTransaction)).CreateResolver();
            IMsSqlDatabaseMigrator msSqlDatabaseMigrator = resolver.Resolve<IMsSqlDatabaseMigrator>();

            MsSqlSagaStoreSetup.Run(msSqlDatabaseMigrator, settings.ConnectionString).Wait();

            return resolver;
        }

        protected IResolver ConfigureServicesWithPostgreSqlSagas(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("PostgreSqlSagasConnectionString");
            PostgreSqlSettings settings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<InMemoryDbContext<IPostgreSqlEntityFrameworkCoreContext>>(s =>
                s.UseInMemoryDatabase("crystalsharp-postgresql-data-inmemory"));
            serviceCollection.AddScoped<IPostgreSqlEntityFrameworkCoreContext, PostgreSqlEntityFrameworkCoreContext>();
            serviceCollection.AddScoped<IInMemoryDataContext>(s =>
                s.GetRequiredService<InMemoryDbContext<IPostgreSqlEntityFrameworkCoreContext>>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddPostgreSqlSagaStore(settings, typeof(PlaceOrderTransaction)).CreateResolver();
            IPostgreSqlDatabaseMigrator postgreSqlDatabaseMigrator = resolver.Resolve<IPostgreSqlDatabaseMigrator>();

            PostgreSqlSagaStoreSetup.Run(postgreSqlDatabaseMigrator, settings.ConnectionString).Wait();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMySqlSagas(IConfigurationRoot configurationRoot)
        {
            string connectionString = configurationRoot.GetConnectionString("MySqlSagasConnectionString");
            MySqlSettings settings = new(connectionString);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<InMemoryDbContext<IMySqlEntityFrameworkCoreContext>>(s => 
                s.UseInMemoryDatabase("crystalsharp-mysql-data-inmemory"));
            serviceCollection.AddScoped<IMySqlEntityFrameworkCoreContext, MySqlEntityFrameworkCoreContext>();
            serviceCollection.AddScoped<IInMemoryDataContext>(s => 
                s.GetRequiredService<InMemoryDbContext<IMySqlEntityFrameworkCoreContext>>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);
            IResolver resolver = crystalSharpAdapter.AddMySqlSagaStore(settings, typeof(PlaceOrderTransaction)).CreateResolver();
            IMySqlDatabaseMigrator mySqlDatabaseMigrator = resolver.Resolve<IMySqlDatabaseMigrator>();

            MySqlSagaStoreSetup.Run(mySqlDatabaseMigrator, settings.ConnectionString).Wait();

            return resolver;
        }

        protected IResolver ConfigureServicesWithMongoDbSagas(IConfigurationRoot configurationRoot, string databaseToUse)
        {
            string connectionString = configurationRoot.GetConnectionString("MongoDbSagasConnectionString");
            MongoDbSettings settings = new(connectionString, databaseToUse);
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<InMemoryDbContext<IMsSqlEntityFrameworkCoreContext>>(s => 
                s.UseInMemoryDatabase("crystalsharp-inmemory"));
            serviceCollection.AddScoped<IMsSqlEntityFrameworkCoreContext, MsSqlEntityFrameworkCoreContext>();
            serviceCollection.AddScoped<IInMemoryDataContext>(s => 
                s.GetRequiredService<InMemoryDbContext<IMsSqlEntityFrameworkCoreContext>>());

            ICrystalSharpAdapter crystalSharpAdapter = ConfigureCrystalSharpAdapter(serviceCollection);

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

        private ICrystalSharpAdapter ConfigureCrystalSharpAdapter(IServiceCollection services)
        {
            return CrystalSharpAdapter.New(services).AddCqrs(typeof(PlaceOrderTransaction));
        }
    }
}
