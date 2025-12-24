namespace CrystalSharp.Messaging.RabbitMq.Configuration
{
    public class RabbitMqSslOptions(string serverName,
        string certificatePath = "",
        bool enabled = false)
    {
        public readonly string ServerName = serverName;
        public readonly string CertificatePath = certificatePath;
        public readonly bool Enabled = enabled;
    }
}
