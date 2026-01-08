using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Infrastructure.EventStoresPersistence;

namespace CrystalSharp.EventStores.KurrentDb.Stores
{
    public class KurrentDbAggregateEventStore<TKey>(
        IResolver resolver,
        IEventStorePersistence eventStorePersistence,
        IEventDispatcher eventDispatcher)
        : AggregateEventStore<TKey>(resolver, eventStorePersistence, eventDispatcher), IAggregateEventStore<TKey>
    {
        //
    }
}
