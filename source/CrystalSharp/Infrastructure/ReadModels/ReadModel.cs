using CrystalSharp.Domain;
using System;

namespace CrystalSharp.Infrastructure.ReadModels
{
    public abstract class ReadModel<TKey> : IReadModel<TKey>
    {
        public TKey Id { get; set; }
        public Guid GlobalUId { get; set; }
        public EntityStatus EntityStatus { get; set; } = EntityStatus.Active;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
