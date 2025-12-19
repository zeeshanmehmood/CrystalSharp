using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SnapshotAttribute(string name, int frequency) : Attribute
    {
        public string Name { get; } = name;
        public int Frequency { get; } = frequency;
    }
}
