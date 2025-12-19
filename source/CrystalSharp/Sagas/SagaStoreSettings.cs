using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;

namespace CrystalSharp.Sagas
{
    public static class SagaStoreSettings
    {
        public static string SagaStoreTable => ReservedTableName.SagaTransaction;
        public static string SagaStoreTableColumns => typeof(SagaTransactionMeta).PropertiesToColumns();
        public static string FormatSagaStoreTableColumns(string prefix, string suffix) => typeof(SagaTransactionMeta).PropertiesToColumns(prefix, suffix);
    }
}
