using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions
{
    public class SnapshotFrequencyException : Exception
    {
        public readonly string Stream;
        public readonly int Frequency;
        public readonly int ErrorCode;

        public SnapshotFrequencyException()
            : base("The snapshot frequency must be greater than zero (\"0\") and cannot be negative.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotFrequencyException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotFrequencyException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SnapshotFrequencyException(string stream, int frequency, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            Frequency = frequency;
            ErrorCode = errorCode;
        }

        public SnapshotFrequencyException(string stream, int frequency, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            Frequency = frequency;
            ErrorCode = errorCode;
        }
    }
}
