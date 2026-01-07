using CrystalSharp.Envoy;
using CrystalSharp.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrystalSharp.Tests.UnitTests.Envoy
{
    public class EnvoyTestFixture : UnitTestBase, IDisposable
    {
        public IEnvoy Envoy { get; private set; }

        public EnvoyTestFixture()
        {
            IResolver resolver = ConfigureCrystalSharpAdapter(new ServiceCollection());
            Envoy = resolver.Resolve<IEnvoy>();
        }

        public void Dispose()
        {
            //
        }
    }
}
