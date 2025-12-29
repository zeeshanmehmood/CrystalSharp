using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate.Events
{
    public class DepartmentDetailsChangedDomainEvent : DomainEvent
    {
        public DepartmentDetails DepartmentDetails { get; set; }

        public DepartmentDetailsChangedDomainEvent(Guid streamId, DepartmentDetails departmentDetails)
        {
            StreamId = streamId;
            DepartmentDetails = departmentDetails;
        }

        [JsonConstructor]
        public DepartmentDetailsChangedDomainEvent(
            Guid streamId,
            DepartmentDetails departmentDetails,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            DepartmentDetails = departmentDetails;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
