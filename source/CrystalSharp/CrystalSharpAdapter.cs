using CrystalSharp.Application.Execution;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Envoy.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace CrystalSharp
{
    public class CrystalSharpAdapter : ICrystalSharpAdapter
    {
        public IServiceCollection ServiceCollection { get; }

        private CrystalSharpAdapter(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;

            RegisterDefaults(ServiceCollection);
        }

        public static ICrystalSharpAdapter New(IServiceCollection serviceCollection) => new CrystalSharpAdapter(serviceCollection);

        public ICrystalSharpAdapter AddCqrs(params Type[] types)
        {
            Assembly[] assemblies = [.. types.Select(t => t.Assembly)];

            ServiceCollection.AddEnvoy(assemblies);
            ServiceCollection.AddScoped<IEventDispatcher, TransactionalDomainEventDispatcher>();
            ServiceCollection.AddScoped<ICommandExecutor, CommandExecutor>();
            ServiceCollection.AddScoped<IQueryExecutor, QueryExecutor>();
            ServiceCollection.AddScoped<INotificationPublisher, NotificationPublisher>();

            return this;
        }

        public IResolver CreateResolver()
        {
            IServiceProvider serviceProvider = ServiceCollection.BuildServiceProvider();

            return serviceProvider.GetService<IResolver>();
        }

        private void RegisterDefaults(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IResolver, ServiceResolver>();
        }
    }
}
