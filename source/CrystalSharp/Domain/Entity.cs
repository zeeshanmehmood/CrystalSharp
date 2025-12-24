using CrystalSharp.Common.Extensions;
using System;

namespace CrystalSharp.Domain
{
    public abstract class Entity<TKey> : IEntity<TKey>
    {
        public virtual TKey Id { get; protected set; }
        public Guid GlobalUId { get; protected set; } = Guid.Create();
        public EntityStatus EntityStatus { get; protected set; } = EntityStatus.Active;
        public DateTime CreatedAt { get; protected set; }
        public DateTime? ModifiedOn { get; protected set; }

        public void SetSecondaryId(Guid globalUId)
        {
            GlobalUId = globalUId;
        }

        public void SetCreatedAt(DateTime createdAt)
        {
            CreatedAt = (CreatedAt == default) ? createdAt : CreatedAt;
        }

        public void SetModifiedOn(DateTime modifiedOn)
        {
            ModifiedOn = modifiedOn;
        }

        public virtual void Activate()
        {
            EntityStatus = EntityStatus.Active;
        }

        public virtual void Delete()
        {
            EntityStatus = EntityStatus.Deleted;
        }

        public bool Active()
        {
            return EntityStatus == EntityStatus.Active;
        }
    }
}
