// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots
{
    public abstract class SnapshotAggregateRoot<TKey, TSnapshot> : AggregateRoot<TKey>, ISnapshotAggregateRoot
        where TSnapshot : class, ISnapshot
    {
        public long SnapshotVersion { get; set; } = -1;

        public virtual async Task<bool> ShouldTakeSnapshot(CancellationToken cancellationToken = default)
        {
            bool takeSnapshot = false;

            if (typeof(TSnapshot).GetCustomAttribute(typeof(SnapshotAttribute)) is SnapshotAttribute snapshotAttribute)
            {
                if (snapshotAttribute.Frequency < 1)
                {
                    string stream = this.GetType().ToStreamName(this.GlobalUId);
                    string errorMessage = "The snapshot frequency must be greater than zero (\"0\") and cannot be negative. Check the Stream and Frequency properties of this exception.";

                    throw new SnapshotFrequencyException(stream, snapshotAttribute.Frequency, ReservedErrorCode.SystemError, errorMessage);
                }

                int frequency = snapshotAttribute.Frequency;
                long aggregateVersion = Version;
                takeSnapshot = ++aggregateVersion % frequency == 0;
            }

            return await Task.FromResult(takeSnapshot);
        }

        public virtual async Task CreateSnapshot(ISnapshotStore snapshotStore, CancellationToken cancellationToken = default)
        {
            bool takeSnapshot = await ShouldTakeSnapshot(cancellationToken).ConfigureAwait(false);

            if (takeSnapshot)
            {
                TSnapshot snapshot = this.CopyTo<TSnapshot>();

                if (snapshot != null)
                {
                    await snapshotStore.SetSnapshot(snapshot, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task<object> LoadSnapshot(ISnapshotStore snapshotStore,
            Guid aggregateStreamId,
            CancellationToken cancellationToken = default)
        {
            TSnapshot snapshot = await LoadSnapshotFromStore(snapshotStore, aggregateStreamId, cancellationToken).ConfigureAwait(false);

            return snapshot;
        }

        public virtual async Task<object> CopySnapshotTo(ISnapshotStore snapshotStore,
            Type aggregateRootType,
            CancellationToken cancellationToken = default)
        {
            TSnapshot snapshot = await LoadSnapshotFromStore(snapshotStore, this.GlobalUId, cancellationToken).ConfigureAwait(false);
            object copy = null;

            if (snapshot != null)
            {
                copy = snapshot.CopyTo(aggregateRootType);
            }

            return copy;
        }

        private async Task<TSnapshot> LoadSnapshotFromStore(ISnapshotStore snapshotStore,
            Guid aggregateStreamId,
            CancellationToken cancellationToken = default)
        {
            TSnapshot snapshot = default;

            try
            {
                snapshot = await snapshotStore.LoadSnapshot<TSnapshot>(aggregateStreamId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (exception is SnapshotNotFoundException || exception is SnapshotDeletedException)
                {
                    snapshot = default;
                }
            }

            return snapshot;
        }
    }
}
