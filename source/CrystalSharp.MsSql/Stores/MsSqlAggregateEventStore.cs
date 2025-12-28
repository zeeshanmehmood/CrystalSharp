using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Infrastructure.EventStoresPersistence;

namespace CrystalSharp.MsSql.Stores
{
    public class MsSqlAggregateEventStore<TKey>(
        IResolver resolver,
        IEventStorePersistence eventStorePersistence,
        IEventDispatcher eventDispatcher) : AggregateEventStore<TKey>(resolver, eventStorePersistence, eventDispatcher), IAggregateEventStore<TKey>
    {
        //
    }
}
