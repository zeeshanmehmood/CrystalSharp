using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreStreamNotFoundException : Exception
    {
        public readonly string Stream;
        public readonly int ErrorCode;

        public EventStoreStreamNotFoundException()
            : base("Stream not found.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreStreamNotFoundException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreStreamNotFoundException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreStreamNotFoundException(string stream, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }

        public EventStoreStreamNotFoundException(string stream, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }
    }
}
