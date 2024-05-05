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
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using CrystalSharp.Application.Execution;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Envoy.Extensions;

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
            Assembly[] assemblies = types.Select(t => t.Assembly).ToArray();

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
