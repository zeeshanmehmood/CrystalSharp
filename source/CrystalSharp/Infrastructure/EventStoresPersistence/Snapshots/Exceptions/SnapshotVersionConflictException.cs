using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions
{
    public class SnapshotVersionConflictException : Exception
    {
        public readonly string Stream;
        public readonly long LastVersion;
        public readonly long ExpectedVersion;
        public readonly int ErrorCode;

        public SnapshotVersionConflictException()
            : base("The expected version of the snapshot already exists.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotVersionConflictException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotVersionConflictException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SnapshotVersionConflictException(string stream, long lastVersion, long expectedVersion, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            LastVersion = lastVersion;
            ExpectedVersion = expectedVersion;
            ErrorCode = errorCode;
        }

        public SnapshotVersionConflictException(
            string stream,
            long lastVersion,
            long expectedVersion,
            int errorCode,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            LastVersion = lastVersion;
            ExpectedVersion = expectedVersion;
            ErrorCode = errorCode;
        }
    }
}
