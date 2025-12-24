using System;
using System.Collections.Generic;

namespace CrystalSharp.Messaging.Distributed.Models
{
    public class GeneralConsumer : IConsumer
    {
        public IList<string> Queues { get; set; } = [];
        public Action<string> Action { get; set; }
    }
}
