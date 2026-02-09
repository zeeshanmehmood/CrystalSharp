using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrystalSharp
{
    public interface ICrystalSharpAdapter
    {
        IServiceCollection ServiceCollection { get; }

        ICrystalSharpAdapter AddCqrs(params Type[] types);
        void Register(Type serviceType, ServiceLifetime serviceLifetime);
        void Register(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime);
        void Register<TService>(ServiceLifetime serviceLifetime);
        void Register<TService, TImplementation>(ServiceLifetime serviceLifetime);
        void Register<TService>(Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime);
        void TryRegister(Type serviceType, ServiceLifetime serviceLifetime);
        void TryRegister(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime);
        void TryRegister<TService>(ServiceLifetime serviceLifetime);
        void TryRegister<TService, TImplementation>(ServiceLifetime serviceLifetime);
        void TryRegister<TService>(Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime);
    }
}
