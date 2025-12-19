using System;

namespace CrystalSharp.Domain
{
    public interface ICreatedAtEntity
    {
        DateTime CreatedAt { get; }
        void SetCreatedAt(DateTime createdAt);
    }
}
