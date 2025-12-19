using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Events
{
    public class CourseNameChangedDomainEvent : DomainEvent
    {
        public string Name { get; set; }

        public CourseNameChangedDomainEvent(Guid streamId, string name)
        {
            StreamId = streamId;
            Name = name;
        }

        [JsonConstructor]
        public CourseNameChangedDomainEvent(Guid streamId,
            string name,
            int entityStatus,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            EntityStatus = entityStatus;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
