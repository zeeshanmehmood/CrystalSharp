using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate.Events
{
    public class CurrencyDetailsChangedDomainEvent : DomainEvent
    {
        public CurrencyDetails CurrencyDetails { get; set; }

        public CurrencyDetailsChangedDomainEvent(Guid streamId, CurrencyDetails currencyDetails)
        {
            StreamId = streamId;
            CurrencyDetails = currencyDetails;
        }

        [JsonConstructor]
        public CurrencyDetailsChangedDomainEvent(
            Guid streamId,
            CurrencyDetails currencyDetails,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            CurrencyDetails = currencyDetails;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
