namespace CrystalSharp.Domain
{
    public interface IEntity<TKey> : IHasSecondaryId, ICreatedAtEntity, IModifiedOnEntity
    {
        TKey Id { get; }
        EntityStatus EntityStatus { get; }
        void Activate();
        void Delete();
        bool Active();
    }
}
