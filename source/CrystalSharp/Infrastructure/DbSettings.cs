namespace CrystalSharp.Infrastructure
{
    public abstract class DbSettings(string connectionString)
    {
        public string ConnectionString { get; } = connectionString;
    }
}
