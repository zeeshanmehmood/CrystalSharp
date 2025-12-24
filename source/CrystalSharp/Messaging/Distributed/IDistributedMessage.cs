namespace CrystalSharp.Messaging.Distributed
{
    public interface IDistributedMessage
    {
        string Body { get; set; }
    }

    public interface IDistributedMessage<T> where T : class
    {
        T Body { get; set; }
    }
}
