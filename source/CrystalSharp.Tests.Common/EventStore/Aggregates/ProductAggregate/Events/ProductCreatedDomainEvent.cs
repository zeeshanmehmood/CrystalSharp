using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.ProductAggregate.Events
{
    public class ProductCreatedDomainEvent : DomainEvent
    {
        public string Name { get; set; }
        public ProductInfo ProductInfo { get; set; }

        public ProductCreatedDomainEvent(Guid streamId, string name, ProductInfo productInfo)
        {
            StreamId = streamId;
            Name = name;
            ProductInfo = productInfo;
        }

        [JsonConstructor]
        public ProductCreatedDomainEvent(Guid streamId,
            string name,
            ProductInfo productInfo,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            ProductInfo = productInfo;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
