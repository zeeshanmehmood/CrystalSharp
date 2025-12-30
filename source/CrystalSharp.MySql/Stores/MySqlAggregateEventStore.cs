using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Infrastructure.EventStoresPersistence;

namespace CrystalSharp.MySql.Stores
{
    public class MySqlAggregateEventStore<TKey>(
        IResolver resolver,
        IEventStorePersistence eventStorePersistence,
        IEventDispatcher eventDispatcher) : AggregateEventStore<TKey>(resolver, eventStorePersistence, eventDispatcher), IAggregateEventStore<TKey>
    {
        //
    }
}
