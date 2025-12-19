using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain.Infrastructure;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public static class EventStoreSettings
    {
        public static string EventStoreTable => ReservedTableName.EventStore;
        public static string EventStoreTableColumns => typeof(DbDomainEvent).PropertiesToColumns();
        public static string FormatEventStoreTableColumns(string prefix, string suffix) => typeof(DbDomainEvent).PropertiesToColumns(prefix, suffix);
    }
}
