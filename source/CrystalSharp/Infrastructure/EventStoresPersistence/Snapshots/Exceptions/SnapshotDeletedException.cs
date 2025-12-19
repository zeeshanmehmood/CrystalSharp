using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions
{
    public class SnapshotDeletedException : Exception
    {
        public readonly string Stream;
        public readonly int ErrorCode;

        public SnapshotDeletedException()
            : base("Cannot read from a deleted snapshot stream.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotDeletedException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotDeletedException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SnapshotDeletedException(string stream, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }

        public SnapshotDeletedException(string stream, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }
    }
}
