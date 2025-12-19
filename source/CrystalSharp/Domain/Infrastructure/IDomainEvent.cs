using CrystalSharp.Envoy.Contracts;
using System;

namespace CrystalSharp.Domain.Infrastructure
{
    public interface IDomainEvent : INotificationMessage
    {
        public Guid StreamId { get; set; }
        string StreamName { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public string EventAssembly { get; set; }
        public int EntityStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime OccurredOn { get; set; }
        public long Version { get; set; }
    }
}
