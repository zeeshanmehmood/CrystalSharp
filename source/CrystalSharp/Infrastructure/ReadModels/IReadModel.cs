using CrystalSharp.Domain;
using System;

namespace CrystalSharp.Infrastructure.ReadModels
{
    public interface IReadModel<TKey>
    {
        TKey Id { get; set; }
        Guid GlobalUId { get; set; }
        EntityStatus EntityStatus { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}
