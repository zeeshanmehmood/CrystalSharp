using System;
using System.Collections.Generic;

namespace CrystalSharp.Messaging.Distributed
{
    public interface IConsumer
    {
        IList<string> Queues { get; set; }
        Action<string> Action { get; set; }
    }
}
