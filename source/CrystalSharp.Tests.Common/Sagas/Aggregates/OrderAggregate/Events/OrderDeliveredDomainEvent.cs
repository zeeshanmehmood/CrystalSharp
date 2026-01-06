using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate.Events
{
    public class OrderDeliveredDomainEvent : DomainEvent
    {
        public bool Delivered { get; set; }

        public OrderDeliveredDomainEvent(Guid streamId, bool delivered)
        {
            StreamId = streamId;
            Delivered = delivered;
        }

        [JsonConstructor]
        public OrderDeliveredDomainEvent(
            Guid streamId,
            bool delivered,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Delivered = delivered;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
