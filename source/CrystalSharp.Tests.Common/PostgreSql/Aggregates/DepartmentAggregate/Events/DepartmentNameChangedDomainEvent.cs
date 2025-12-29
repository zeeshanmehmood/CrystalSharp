using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate.Events
{
    public class DepartmentNameChangedDomainEvent : DomainEvent
    {
        public string Name { get; set; }

        public DepartmentNameChangedDomainEvent(Guid streamId, string name)
        {
            StreamId = streamId;
            Name = name;
        }

        [JsonConstructor]
        public DepartmentNameChangedDomainEvent(
            Guid streamId,
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
