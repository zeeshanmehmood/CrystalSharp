using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate.Events
{
    public class OrderDeletedDomainEvent : DomainEvent
    {
        public OrderDeletedDomainEvent(Guid streamId)
        {
            StreamId = streamId;
        }

        [JsonConstructor]
        public OrderDeletedDomainEvent(
            Guid streamId,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
