using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate.Events
{
    public class SupplierDeletedDomainEvent : DomainEvent
    {
        public string Name { get; set; }
        public SupplierDetails SupplierDetails { get; set; }

        public SupplierDeletedDomainEvent(Guid streamId, string name, SupplierDetails supplierDetails)
        {
            StreamId = streamId;
            Name = name;
            SupplierDetails = supplierDetails;
        }

        [JsonConstructor]
        public SupplierDeletedDomainEvent(
            Guid streamId,
            string name,
            SupplierDetails supplierDetails,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            SupplierDetails = supplierDetails;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
