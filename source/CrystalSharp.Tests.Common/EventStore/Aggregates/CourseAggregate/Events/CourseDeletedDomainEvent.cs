using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Events
{
    public class CourseDeletedDomainEvent : DomainEvent
    {
        public CourseDeletedDomainEvent(Guid streamId)
        {
            StreamId = streamId;
        }

        [JsonConstructor]
        public CourseDeletedDomainEvent(Guid streamId,
            int entityStatus,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            EntityStatus = entityStatus;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
