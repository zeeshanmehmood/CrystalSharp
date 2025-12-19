using System;

namespace CrystalSharp.Domain.Infrastructure
{
    public interface IDbDomainEvent : IDomainEvent
    {
        public Guid Id { get; set; }
        long GlobalSequence { get; set; }
        long Sequence { get; set; }
        string Data { get; set; }
    }
}
