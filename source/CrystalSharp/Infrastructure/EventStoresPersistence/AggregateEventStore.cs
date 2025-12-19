using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Domain.EventDispatching;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public abstract class AggregateEventStore<TKey>(IResolver resolver, IEventStorePersistence eventStorePersistence, IEventDispatcher eventDispatcher)
    {
        private const string AGGREGATE_CLR_TYPE_HEADER = ReservedName.AggregateClrTypeName;
        private const string COMMIT_ID_HEADER = ReservedName.CommitId;

        private readonly IResolver _resolver = resolver;
        private readonly IEventStorePersistence _eventStorePersistence = eventStorePersistence;
        private readonly IEventDispatcher _eventDispatcher = eventDispatcher;

        protected TAggregate ConstructAggregate<TAggregate>()
        {
            return Activator.CreateInstance<TAggregate>();
        }

        protected virtual async Task<TAggregate> GetAllEvents<TAggregate>(Guid streamId, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            string stream = _eventStorePersistence.IdToStreamName(typeof(TAggregate), streamId);
            TAggregate aggregate = ConstructAggregate<TAggregate>();
            IEnumerable<IDomainEvent> domainEvents = await _eventStorePersistence.Get<IDomainEvent>(stream, cancellationToken).ConfigureAwait(false);

            if (domainEvents.HasAny())
            {
                aggregate.LoadStateFromHistory(domainEvents);
            }

            return aggregate;
        }

        protected virtual async Task<TAggregate> GetAggregateByVersion<TAggregate>(Guid streamId, long version,
            TAggregate aggregate,
            CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            string stream = _eventStorePersistence.IdToStreamName(typeof(TAggregate), streamId);
            IDomainEvent domainEvent = await _eventStorePersistence.GetByVersion<IDomainEvent>(stream, version, cancellationToken).ConfigureAwait(false);

            if (domainEvent is not null)
            {
                IList<IDomainEvent> domainEvents = [domainEvent];

                aggregate.LoadStateFromHistory(domainEvents);
            }

            if (aggregate.Version != version && version < long.MaxValue)
            {
                string errorMessage = "Invalid aggregate version.";

                throw new AggregateVersionException(aggregate.Version, version, ReservedErrorCode.SystemError, errorMessage);
            }

            return aggregate;
        }

        public async Task<TAggregate> Get<TAggregate>(Guid streamId, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            TAggregate aggregate = ConstructAggregate<TAggregate>();

            ((IEntity<TKey>)aggregate).SetSecondaryId(streamId);

            string stream = _eventStorePersistence.IdToStreamName(typeof(TAggregate), streamId);
            bool loadState = false;

            if (IsSnapshotStoreRegistered() && IsSnapshotAggregateRoot(aggregate))
            {
                long startVersion = await GetAggregateVersionFromSnapshot(aggregate, streamId, cancellationToken).ConfigureAwait(false);
                bool loadFromSnapshot = startVersion > -1;

                if (loadFromSnapshot)
                {
                    IDomainEvent lastAggregateEvent = await _eventStorePersistence.GetLastEvent<IDomainEvent>(stream, cancellationToken).ConfigureAwait(false);

                    if (lastAggregateEvent is not null)
                    {
                        aggregate = await GetAggregateFromSnapshot(aggregate, cancellationToken).ConfigureAwait(false);
                        long endVersion = lastAggregateEvent.Version;

                        while (startVersion < endVersion)
                        {
                            long version = ++startVersion;
                            aggregate = await GetByVersion(streamId, version, aggregate, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    loadState = true;
                }
            }

            if (!loadState)
            {
                aggregate = await GetAllEvents<TAggregate>(streamId, cancellationToken).ConfigureAwait(false);
            }

            return aggregate;
        }

        public async Task<TAggregate> GetByVersion<TAggregate>(Guid streamId, long version, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            TAggregate aggregate = ConstructAggregate<TAggregate>();
            aggregate = await GetAggregateByVersion(streamId, version, aggregate, cancellationToken).ConfigureAwait(false);

            return aggregate;
        }

        public async Task<TAggregate> GetByVersion<TAggregate>(Guid streamId,
            long version,
            TAggregate aggregate,
            CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            return await GetAggregateByVersion(streamId, version, aggregate, cancellationToken).ConfigureAwait(false);
        }

        public async Task Store<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            if (!aggregate.UncommittedEvents().HasAny())
            {
                string errorMessage = "There are no events to store.";

                throw new ZeroEventsException(ReservedErrorCode.SystemError, errorMessage);
            }

            IDictionary<string, object> headers = new Dictionary<string, object>
            {
                { COMMIT_ID_HEADER, aggregate.GlobalUId },
                { AGGREGATE_CLR_TYPE_HEADER, aggregate.GetType().AssemblyQualifiedName }
            };
            string stream = _eventStorePersistence.IdToStreamName(aggregate.GetType(), aggregate.GlobalUId);
            int uncommittedEvents = aggregate.UncommittedEvents().Count;
            var originalVersion = aggregate.Version - uncommittedEvents;
            long expectedVersion = _eventStorePersistence.GetExpectedVersion(originalVersion);
            List<IDomainEvent> events = [.. aggregate.UncommittedEvents()];

            foreach (IDomainEvent @event in events)
            {
                @event.StreamName = stream;
            }

            IEnumerable<EventDataItem<IDomainEvent>> eventsData = _eventStorePersistence.PrepareEventData(events, headers);

            await _eventStorePersistence.Store(stream, eventsData, expectedVersion, cancellationToken).ConfigureAwait(false);
            await CreateSnapshotIfNeeded(aggregate, cancellationToken).ConfigureAwait(false);

            IReadOnlyList<IDomainEvent> domainEvents = aggregate.UncommittedEvents();

            if (domainEvents.HasAny())
            {
                await _eventDispatcher.Dispatch(domainEvents, cancellationToken).ConfigureAwait(false);
            }

            aggregate.MarkEventsAsCommitted();
        }

        public async Task Delete<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            IReadOnlyList<IDomainEvent> domainEvents = aggregate.UncommittedEvents();

            await Delete<TAggregate>(aggregate.GlobalUId, cancellationToken).ConfigureAwait(false);

            if (domainEvents.HasAny())
            {
                await _eventDispatcher.Dispatch(domainEvents, cancellationToken).ConfigureAwait(false);
            }

            aggregate.MarkEventsAsCommitted();
        }

        public async Task Delete<TAggregate>(Guid streamId, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            string stream = _eventStorePersistence.IdToStreamName(typeof(TAggregate), streamId);

            await _eventStorePersistence.Delete(stream, cancellationToken).ConfigureAwait(false);
        }

        private async Task CreateSnapshotIfNeeded<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            if (aggregate is ISnapshotAggregateRoot snapshotAggregateRoot)
            {
                if (IsSnapshotStoreRegistered())
                {
                    ISnapshotStore snapshotStore = _resolver.Resolve<ISnapshotStore>();

                    await snapshotAggregateRoot.CreateSnapshot(snapshotStore, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private bool IsSnapshotAggregateRoot<TAggregate>(TAggregate aggregate)
            where TAggregate : IAggregateRoot<TKey>
        {
            return aggregate is ISnapshotAggregateRoot;
        }

        private bool IsSnapshotStoreRegistered()
        {
            return _resolver.IsRegistered<ISnapshotStore>();
        }

        private async Task<long> GetAggregateVersionFromSnapshot<TAggregate>(TAggregate aggregate,
            Guid aggregateStreamId,
            CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            long version = -1;

            if (aggregate is ISnapshotAggregateRoot snapshotAggregateRoot)
            {
                if (IsSnapshotStoreRegistered())
                {
                    ISnapshotStore snapshotStore = _resolver.Resolve<ISnapshotStore>();
                    ISnapshot snapshot = await snapshotAggregateRoot.LoadSnapshot(snapshotStore,
                        aggregateStreamId,
                        cancellationToken)
                        .ConfigureAwait(false) as ISnapshot;
                    version = (snapshot == null) ? -1 : snapshot.Version;
                }
            }

            return version;
        }

        private async Task<TAggregate> GetAggregateFromSnapshot<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : IAggregateRoot<TKey>
        {
            if (aggregate is ISnapshotAggregateRoot snapshotAggregateRoot)
            {
                if (IsSnapshotStoreRegistered())
                {
                    ISnapshotStore snapshotStore = _resolver.Resolve<ISnapshotStore>();
                    object snapshot = await snapshotAggregateRoot.CopySnapshotTo(snapshotStore, aggregate.GetType(), cancellationToken).ConfigureAwait(false);

                    if (snapshot is not null)
                    {
                        aggregate = snapshot.CopyTo<TAggregate>();
                    }
                }
            }

            return aggregate;
        }
    }
}
