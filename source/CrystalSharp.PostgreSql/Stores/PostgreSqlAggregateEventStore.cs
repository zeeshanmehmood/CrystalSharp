using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Infrastructure.EventStoresPersistence;

namespace CrystalSharp.PostgreSql.Stores
{
    public class PostgreSqlAggregateEventStore<TKey>(
        IResolver resolver,
        IEventStorePersistence eventStorePersistence,
        IEventDispatcher eventDispatcher) : AggregateEventStore<TKey>(resolver, eventStorePersistence, eventDispatcher), IAggregateEventStore<TKey>
    {
        //
    }
}
