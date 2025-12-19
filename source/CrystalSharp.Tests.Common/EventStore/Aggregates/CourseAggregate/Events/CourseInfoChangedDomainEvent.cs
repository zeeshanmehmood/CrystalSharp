using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Events
{
    public class CourseInfoChangedDomainEvent : DomainEvent
    {
        public CourseInfo CourseInfo { get; set; }

        public CourseInfoChangedDomainEvent(Guid streamId, CourseInfo courseInfo)
        {
            StreamId = streamId;
            CourseInfo = courseInfo;
        }

        [JsonConstructor]
        public CourseInfoChangedDomainEvent(Guid streamId,
            CourseInfo courseInfo,
            int entityStatus,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            CourseInfo = courseInfo;
            EntityStatus = entityStatus;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
