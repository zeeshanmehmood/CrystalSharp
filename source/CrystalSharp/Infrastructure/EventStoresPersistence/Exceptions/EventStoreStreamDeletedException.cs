using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreStreamDeletedException : Exception
    {
        public readonly string Stream;
        public readonly int ErrorCode;

        public EventStoreStreamDeletedException()
            : base("Cannot read from a deleted stream.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreStreamDeletedException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreStreamDeletedException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreStreamDeletedException(string stream, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }

        public EventStoreStreamDeletedException(string stream, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }
    }
}
