using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreVersionConflictException : Exception
    {
        public readonly string Stream;
        public readonly long LastVersion;
        public readonly long ExpectedVersion;
        public readonly int ErrorCode;

        public EventStoreVersionConflictException()
            : base("The expected version already exists.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreVersionConflictException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreVersionConflictException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreVersionConflictException(string stream, long lastVersion, long expectedVersion, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            LastVersion = lastVersion;
            ExpectedVersion = expectedVersion;
            ErrorCode = errorCode;
        }

        public EventStoreVersionConflictException(
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
