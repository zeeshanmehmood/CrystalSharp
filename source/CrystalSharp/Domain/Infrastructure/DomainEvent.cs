using System;

namespace CrystalSharp.Domain.Infrastructure
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid StreamId { get; set; }
        public string StreamName { get; set; }
        public Guid EventId { get; set; } = Guid.CreateVersion7();
        public string EventType { get; set; }
        public string EventAssembly { get; set; }
        public int EntityStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime OccurredOn { get; set; }
        public long Version { get; set; }
    }
}
