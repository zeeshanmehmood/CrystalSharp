using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate.Events
{
    public class CurrencyNameChangedDomainEvent : DomainEvent
    {
        public string Name { get; set; }

        public CurrencyNameChangedDomainEvent(Guid streamId, string name)
        {
            StreamId = streamId;
            Name = name;
        }

        [JsonConstructor]
        public CurrencyNameChangedDomainEvent(Guid streamId,
            string name,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
