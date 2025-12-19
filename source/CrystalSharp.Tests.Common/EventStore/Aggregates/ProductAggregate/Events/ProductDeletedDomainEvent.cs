using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate.Events
{
    public class ProductDeletedDomainEvent : DomainEvent
    {
        public ProductDeletedDomainEvent(Guid streamId)
        {
            StreamId = streamId;
        }

        [JsonConstructor]
        public ProductDeletedDomainEvent(Guid streamId,
            int entityStatus,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            EntityStatus = entityStatus;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
