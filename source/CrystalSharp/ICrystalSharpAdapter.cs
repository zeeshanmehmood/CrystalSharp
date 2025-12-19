using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrystalSharp
{
    public interface ICrystalSharpAdapter
    {
        IServiceCollection ServiceCollection { get; }

        ICrystalSharpAdapter AddCqrs(params Type[] types);
        IResolver CreateResolver();
    }
}
