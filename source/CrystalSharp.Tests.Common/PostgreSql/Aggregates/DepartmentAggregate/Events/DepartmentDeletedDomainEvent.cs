using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate.Events
{
    public class DepartmentDeletedDomainEvent : DomainEvent
    {
        public string Name { get; set; }
        public DepartmentDetails DepartmentDetails { get; set; }

        public DepartmentDeletedDomainEvent(Guid streamId, string name, DepartmentDetails departmentDetails)
        {
            StreamId = streamId;
            Name = name;
            DepartmentDetails = departmentDetails;
        }

        [JsonConstructor]
        public DepartmentDeletedDomainEvent(
            Guid streamId,
            string name,
            DepartmentDetails departmentDetails,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            DepartmentDetails = departmentDetails;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
