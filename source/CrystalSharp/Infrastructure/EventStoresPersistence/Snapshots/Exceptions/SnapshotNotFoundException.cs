using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions
{
    public class SnapshotNotFoundException : Exception
    {
        public readonly string Stream;
        public readonly int ErrorCode;

        public SnapshotNotFoundException()
            : base("Snapshot stream not found.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotNotFoundException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotNotFoundException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SnapshotNotFoundException(string stream, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }

        public SnapshotNotFoundException(string stream, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }
    }
}
