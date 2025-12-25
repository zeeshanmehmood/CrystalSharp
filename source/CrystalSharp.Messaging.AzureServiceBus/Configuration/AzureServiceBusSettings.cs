namespace CrystalSharp.Messaging.AzureServiceBus.Configuration
{
    public class AzureServiceBusSettings(string connectionString)
    {
        public string ConnectionString { get; } = connectionString;
    }
}
