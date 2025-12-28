using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate.Events
{
    public class CurrencyDeletedDomainEvent : DomainEvent
    {
        public string Name { get; set; }
        public CurrencyDetails CurrencyDetails { get; set; }

        public CurrencyDeletedDomainEvent(Guid streamId, string name, CurrencyDetails currencyDetails)
        {
            StreamId = streamId;
            Name = name;
            CurrencyDetails = currencyDetails;
        }

        [JsonConstructor]
        public CurrencyDeletedDomainEvent(
            Guid streamId,
            string name,
            CurrencyDetails currencyDetails,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            CurrencyDetails = currencyDetails;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
