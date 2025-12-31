using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate.Events
{
    public class EmployeeCreatedDomainEvent : DomainEvent
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public EmployeeCreatedDomainEvent(Guid streamId, string name, string code)
        {
            StreamId = streamId;
            Name = name;
            Code = code;
        }

        [JsonConstructor]
        public EmployeeCreatedDomainEvent(
            Guid streamId,
            string name,
            string code,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            Code = code;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
