using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate.Events
{
    public class ProductNameChangedDomainEvent : DomainEvent
    {
        public string Name { get; set; }

        public ProductNameChangedDomainEvent(Guid streamId, string name)
        {
            StreamId = streamId;
            Name = name;
        }

        [JsonConstructor]
        public ProductNameChangedDomainEvent(Guid streamId,
            string name,
            int entityStatus,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            EntityStatus = entityStatus;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
