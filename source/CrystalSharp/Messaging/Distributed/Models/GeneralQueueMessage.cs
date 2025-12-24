namespace CrystalSharp.Messaging.Distributed.Models
{
    public class GeneralQueueMessage : QueueMessage, IDistributedMessage
    {
        public string Body { get; set; }
    }

    public class GeneralQueueMessage<T> : QueueMessage, IDistributedMessage<T> where T : class
    {
        public T Body { get; set; }
    }
}
