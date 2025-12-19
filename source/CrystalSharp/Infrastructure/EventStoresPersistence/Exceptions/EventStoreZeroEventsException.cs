using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreZeroEventsException : Exception
    {
        public readonly string Stream;
        public readonly int ErrorCode;

        public EventStoreZeroEventsException()
            : base("There are no events to store.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreZeroEventsException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreZeroEventsException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreZeroEventsException(string stream, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }

        public EventStoreZeroEventsException(string stream, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }
    }
}
