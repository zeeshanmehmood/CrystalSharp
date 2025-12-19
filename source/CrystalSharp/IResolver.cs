using System;

namespace CrystalSharp
{
    public interface IResolver
    {
        T Resolve<T>();
        bool IsRegistered<T>();
        T CreateInstance<T>(Type type);
    }
}
