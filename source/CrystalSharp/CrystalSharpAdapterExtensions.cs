using CrystalSharp.Common.Extensions;
using CrystalSharp.Sagas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrystalSharp
{
    public static class CrystalSharpAdapterExtensions
    {
        public static void RegisterSagas(this ICrystalSharpAdapter crystalSharpAdapter, params Assembly[] assemblies)
        {
            Type sagaLocatorInterfaceType = typeof(ISagaLocator);
            Type sagaTransactionHandlerType = typeof(SagaTransactionHandler<>);

            crystalSharpAdapter.ServiceCollection.AddScoped<ISagaTransactionExecutor, SagaTransactionExecutor>();

            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<Type> sagaLocatorTypes = assembly.GetTypes().Where(t =>
                    !t.GetTypeInfo().IsAbstract
                    && t.GetTypeInfo().ImplementedInterfaces.Contains(sagaLocatorInterfaceType));

                IEnumerable<Type> sagaTypes = assembly.GetTypes().Where(t =>
                    t.BaseType is not null
                    && t.BaseType.BaseType is not null
                    && t.BaseType.BaseType.Name.IsEqual(sagaTransactionHandlerType.Name));

                if (sagaLocatorTypes.HasAny())
                {
                    foreach (Type sagaLocatorType in sagaLocatorTypes)
                    {
                        crystalSharpAdapter.ServiceCollection.TryAddTransient(sagaLocatorType);
                    }
                }

                if (sagaTypes.HasAny())
                {
                    foreach (Type sagaType in sagaTypes)
                    {
                        crystalSharpAdapter.ServiceCollection.TryAddTransient(sagaType);
                    }
                }
            }
        }
    }
}
