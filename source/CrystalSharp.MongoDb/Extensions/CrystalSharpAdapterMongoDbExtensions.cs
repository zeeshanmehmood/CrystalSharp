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
            crystalSharpAdapter.Register<IMongoDbContext>(s =>
            {
                return new MongoDbContext(settings.ConnectionString, settings.Database, s.GetRequiredService<IEventDispatcher>());
            },
            ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbEventStoreDb<TKey>(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            RegisterBsonSerializerIfRequired();
            MongoDbEventStoreSetup.Run(settings.ConnectionString, settings.Database);

            crystalSharpAdapter.Register<IEventStorePersistence>(s => 
            {
                return new MongoDbEventStore(settings.ConnectionString, settings.Database);
            },
            ServiceLifetime.Scoped);
            
            crystalSharpAdapter.Register<ISnapshotStore>(s =>
            {
                return new MongoDbSnapshotStore(settings.ConnectionString, settings.Database);
            },
            ServiceLifetime.Scoped);

            crystalSharpAdapter.Register<IAggregateEventStore<TKey>>(s =>
            {
                return new MongoDbAggregateEventStore<TKey>(
                    s.GetRequiredService<IResolver>(),
                    s.GetRequiredService<IEventStorePersistence>(),
                    s.GetRequiredService<IEventDispatcher>());
            },
            ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddMongoDbReadModelStore(this ICrystalSharpAdapter crystalSharpAdapter, MongoDbSettings settings)
        {
            RegisterBsonSerializerIfRequired();
            crystalSharpAdapter.Register<IReadModelStore<string>>(s =>
            {
                return new MongoDbReadModelStore(settings.ConnectionString, settings.Database);
            },
            ServiceLifetime.Scoped);

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

            crystalSharpAdapter.Register<IMongoDbSagaContext>(s =>
            {
                return new MongoDbSagaContext(settings.ConnectionString, settings.Database);
            },
            ServiceLifetime.Scoped);
            
            crystalSharpAdapter.Register<ISagaStore, MongoDbSagaStore>(ServiceLifetime.Scoped);

            return crystalSharpAdapter;
        }

        private static void RegisterBsonSerializerIfRequired()
        {
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
    }
}
