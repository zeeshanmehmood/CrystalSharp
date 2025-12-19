using System.Collections.Generic;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public class EventDataItem<TEvent> where TEvent : class
    {
        public IDictionary<string, object> Headers { get; set; }
        public TEvent Event { get; set; }
    }
}
