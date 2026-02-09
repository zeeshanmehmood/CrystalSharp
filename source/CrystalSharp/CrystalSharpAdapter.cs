using CrystalSharp.Application.Execution;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Envoy.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            RegisterService<IEventDispatcher, TransactionalDomainEventDispatcher>(ServiceLifetime.Scoped, true);
            RegisterService<ICommandExecutor, CommandExecutor>(ServiceLifetime.Scoped, true);
            RegisterService<IQueryExecutor, QueryExecutor>(ServiceLifetime.Scoped, true);
            RegisterService<INotificationPublisher, NotificationPublisher>(ServiceLifetime.Scoped, true);

            return this;
        }

        public void Register(Type serviceType, ServiceLifetime serviceLifetime)
        {
            RegisterService(serviceType, serviceLifetime, true);
        }

        public void Register(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime)
        {
            RegisterService(serviceType, implementationType, serviceLifetime, true);
        }

        public void Register<TService>(ServiceLifetime serviceLifetime)
        {
            RegisterService<TService>(serviceLifetime, true);
        }

        public void Register<TService, TImplementation>(ServiceLifetime serviceLifetime)
        {
            RegisterService<TService, TImplementation>(serviceLifetime, true);
        }

        public void Register<TService>(Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime)
        {
            RegisterService<TService>(implementationFactory, serviceLifetime, true);
        }

        public void TryRegister(Type serviceType, ServiceLifetime serviceLifetime)
        {
            RegisterService(serviceType, serviceLifetime, false);
        }

        public void TryRegister(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime)
        {
            RegisterService(serviceType, implementationType, serviceLifetime, false);
        }

        public void TryRegister<TService>(ServiceLifetime serviceLifetime)
        {
            RegisterService<TService>(serviceLifetime, false);
        }

        public void TryRegister<TService, TImplementation>(ServiceLifetime serviceLifetime)
        {
            RegisterService<TService, TImplementation>(serviceLifetime, false);
        }

        public void TryRegister<TService>(Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime)
        {
            RegisterService<TService>(implementationFactory, serviceLifetime, false);
        }

        private void RegisterDefaults(IServiceCollection serviceCollection)
        {
            RegisterService<IResolver, ServiceResolver>(ServiceLifetime.Scoped, true);
        }

        private void RegisterService(Type serviceType, ServiceLifetime serviceLifetime, bool overwrite)
        {
            RegisterServiceWithLifetime(serviceType, serviceLifetime, overwrite);
        }

        private void RegisterService(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime, bool overwrite = true)
        {
            RegisterServiceWithLifetime(serviceType, implementationType, serviceLifetime, overwrite);
        }

        private void RegisterService(Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime, bool overwrite = true)
        {
            RegisterServiceWithLifetime(serviceType, implementationFactory, serviceLifetime, overwrite);
        }

        private void RegisterService<TService>(ServiceLifetime serviceLifetime, bool overwrite)
        {
            RegisterService(typeof(TService), serviceLifetime, overwrite);
        }

        private void RegisterService<TService, TImplementation>(ServiceLifetime serviceLifetime, bool overwrite)
        {
            RegisterService(typeof(TService), typeof(TImplementation), serviceLifetime, overwrite);
        }

        private void RegisterService<TService>(Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime, bool overwrite)
        {
            RegisterService(typeof(TService), implementationFactory, serviceLifetime, overwrite);
        }

        private void RegisterServiceWithLifetime(Type serviceType, ServiceLifetime serviceLifetime, bool overwrite)
        {
            ServiceDescriptor serviceDescriptor = new(serviceType, serviceType, serviceLifetime);

            if (overwrite)
            {
                ServiceCollection.Add(serviceDescriptor);
            }
            else
            {
                ServiceCollection.TryAdd(serviceDescriptor);
            }
        }

        private void RegisterServiceWithLifetime(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime, bool overwrite)
        {
            ServiceDescriptor serviceDescriptor = new(serviceType, implementationType, serviceLifetime);

            if (overwrite)
            {
                ServiceCollection.Add(serviceDescriptor);
            }
            else
            {
                ServiceCollection.TryAdd(serviceDescriptor);
            }
        }

        private void RegisterServiceWithLifetime(Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime, bool overwrite)
        {
            ServiceDescriptor serviceDescriptor = new(serviceType, implementationFactory, serviceLifetime);

            if (overwrite)
            {
                ServiceCollection.Add(serviceDescriptor);
            }
            else
            {
                ServiceCollection.TryAdd(serviceDescriptor);
            }
        }
    }
}
