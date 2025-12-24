using System.Collections.Generic;

namespace CrystalSharp.Messaging.Distributed.Models
{
    public class GeneralExchange
    {
        public string Name { get; set; }
        public string Type { get; set; } = "direct";
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; }
        public string DeadLetterExchange { get; set; } = "default.dlx.exchange";
        public int RequeueAttempts { get; set; } = 2;
        public int RequeueTimeoutMilliseconds { get; set; } = 200;
        public string RoutingKey { get; set; }
        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
        public IList<GeneralQueue> Queues { get; set; } = [];
    }
}
