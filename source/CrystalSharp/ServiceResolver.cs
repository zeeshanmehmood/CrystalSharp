using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrystalSharp
{
    public class ServiceResolver(IServiceProvider serviceProvider) : IResolver
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public T Resolve<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        public bool IsRegistered<T>()
        {
            return _serviceProvider.GetRequiredService<T>() is not null;
        }

        public T CreateInstance<T>(Type type)
        {
            return (T)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}
