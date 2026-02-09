using CrystalSharp.Tests.Common.Envoy.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrystalSharp.Tests.Common
{
    public class UnitTestBase
    {
        protected UnitTestBase()
        {
            //
        }

        protected IResolver ConfigureCrystalSharpAdapter(IServiceCollection services)
        {
            ICrystalSharpAdapter crystalSharpAdapter = CrystalSharpAdapter.New(services).AddCqrs(typeof(CreateProductRequest));
            IServiceProvider serviceProvider = crystalSharpAdapter.ServiceCollection.BuildServiceProvider();

            return serviceProvider.GetRequiredService<IResolver>();
        }
    }
}
