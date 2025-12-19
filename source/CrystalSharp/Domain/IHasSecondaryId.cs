using System;

namespace CrystalSharp.Domain
{
    public interface IHasSecondaryId
    {
        Guid GlobalUId { get; }
        void SetSecondaryId(Guid globalUId);
    }
}
