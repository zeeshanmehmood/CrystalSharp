using System;

namespace CrystalSharp.Domain
{
    public interface IModifiedOnEntity
    {
        DateTime? ModifiedOn { get; }
        void SetModifiedOn(DateTime modifiedOn);
    }
}
