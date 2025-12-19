using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Events
{
    public class CourseCreatedDomainEvent : DomainEvent
    {
        public string Name { get; set; }
        public CourseInfo CourseInfo { get; set; }

        public CourseCreatedDomainEvent(Guid streamId, string name, CourseInfo courseInfo)
        {
            StreamId = streamId;
            Name = name;
            CourseInfo = courseInfo;
        }

        [JsonConstructor]
        public CourseCreatedDomainEvent(Guid streamId,
            string name,
            CourseInfo courseInfo,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Name = name;
            CourseInfo = courseInfo;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
