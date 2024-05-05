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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Sagas;

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
                    t.BaseType != null
                    && t.BaseType.BaseType != null
                    && t.BaseType.BaseType.Name == sagaTransactionHandlerType.Name);

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
