using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate.Events
{
    public class SupplierDetailsChangedDomainEvent : DomainEvent
    {
        public SupplierDetails SupplierDetails { get; set; }

        public SupplierDetailsChangedDomainEvent(Guid streamId, SupplierDetails supplierDetails)
        {
            StreamId = streamId;
            SupplierDetails = supplierDetails;
        }

        [JsonConstructor]
        public SupplierDetailsChangedDomainEvent(
            Guid streamId,
            SupplierDetails supplierDetails,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            SupplierDetails = supplierDetails;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
