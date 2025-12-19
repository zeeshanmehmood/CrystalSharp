using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreEventDeserializationException : Exception
    {
        public readonly string EventTypeName;
        public readonly string Payload;
        public readonly int ErrorCode;

        public EventStoreEventDeserializationException()
            : base("The event cannot be deserialized.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreEventDeserializationException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreEventDeserializationException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreEventDeserializationException(string eventTypeName, string payload, int errorCode, string message)
            : base(message)
        {
            EventTypeName = eventTypeName;
            Payload = payload;
            ErrorCode = errorCode;
        }

        public EventStoreEventDeserializationException(string eventTypeName, string payload, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            EventTypeName = eventTypeName;
            Payload = payload;
            ErrorCode = errorCode;
        }
    }
}
