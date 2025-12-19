using CrystalSharp.Tests.Common.Envoy.Requests;
using Microsoft.Extensions.DependencyInjection;

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
            return CrystalSharpAdapter.New(services)
                .AddCqrs(typeof(CreateProductRequest))
                .CreateResolver();
        }
    }
}
