namespace CrystalSharp.Messaging.Distributed.Models
{
    public class GeneralExchangeMessage : ExchangeMessage, IDistributedMessage
    {
        public string Body { get; set; }
    }

    public class GeneralExchangeMessage<T> : ExchangeMessage, IDistributedMessage<T> where T : class
    {
        public T Body { get; set; }
    }
}
