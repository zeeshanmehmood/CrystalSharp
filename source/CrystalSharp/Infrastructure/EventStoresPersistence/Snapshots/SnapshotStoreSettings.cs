using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public static class SnapshotStoreSettings
    {
        public static string SnapshotTable => ReservedTableName.SnapshotStore;
        public static string SnapshotTableColumns => typeof(DbSnapshotEntity).PropertiesToColumns();
        public static string FormatSnapshotTableColumns(string prefix, string suffix) => typeof(DbSnapshotEntity).PropertiesToColumns(prefix, suffix);
    }
}
