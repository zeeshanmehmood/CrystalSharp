using System;

namespace CrystalSharp.Domain.Infrastructure
{
    public class DbDomainEvent : DomainEvent, IDbDomainEvent
    {
        public Guid Id { get; set; }
        public long GlobalSequence { get; set; }
        public long Sequence { get; set; }
        public string Data { get; set; }
    }
}
