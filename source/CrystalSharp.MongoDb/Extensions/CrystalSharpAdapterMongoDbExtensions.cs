using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.MongoDb.Database;
using CrystalSharp.MongoDb.Settings;
using CrystalSharp.MongoDb.Stores;
using CrystalSharp.Sagas;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Linq;
using System.Reflection;

namespace CrystalSharp.MongoDb.Extensions
{
    public static class CrystalSharpAdapterMongoDbExtensions
    {
        public static ICrystalSharpAdapter AddMongoDb(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            RegisterBsonSerializerIfRequired();
            crystalSharpAdapter.ServiceCollection.AddScoped<IMongoDbContext>(s => 
                new MongoDbContext(settings.ConnectionString,
                settings.Database,
                s.GetRequiredService<IEventDispatcher>()));

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            RegisterBsonSerializerIfRequired();
            MongoDbEventStoreSetup.Run(settings.ConnectionString, settings.Database);

            crystalSharpAdapter.ServiceCollection.AddScoped<IEventStorePersistence>(s => new MongoDbEventStore(settings.ConnectionString, settings.Database));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISnapshotStore>(s => new MongoDbSnapshotStore(settings.ConnectionString, settings.Database));
            crystalSharpAdapter.ServiceCollection.AddScoped<IAggregateEventStore<TKey>>(s =>
                new MongoDbAggregateEventStore<TKey>(s.GetRequiredService<IResolver>(),
                s.GetRequiredService<IEventStorePersistence>(),
                s.GetRequiredService<IEventDispatcher>()));

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbReadModelStore(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            RegisterBsonSerializerIfRequired();
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<string>>(s => new MongoDbReadModelStore(settings.ConnectionString, settings.Database));

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbSagaStore(
            this ICrystalSharpAdapter crystalSharpAdapter,
            MongoDbSettings settings,
            params Type[] types)
        {
            RegisterBsonSerializerIfRequired();
            Assembly[] assemblies = [.. types.Select(t => t.Assembly)];

            crystalSharpAdapter.RegisterSagas(assemblies);

            crystalSharpAdapter.ServiceCollection.AddScoped<IMongoDbSagaContext>(s => new MongoDbSagaContext(settings.ConnectionString, settings.Database));
            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaStore, MongoDbSagaStore>();

            return crystalSharpAdapter;
        }

        private static void RegisterBsonSerializerIfRequired()
        {
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
    }
}
