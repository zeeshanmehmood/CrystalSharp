using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Domain.Exceptions
{
    public class EventDeserializationException : Exception
    {
        public readonly string EventTypeName;
        public readonly string Metadata;
        public readonly int ErrorCode;

        public EventDeserializationException()
            : base("The event cannot be deserialized.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventDeserializationException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventDeserializationException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventDeserializationException(string eventTypeName, string metadata, int errorCode, string message)
            : base(message)
        {
            EventTypeName = eventTypeName;
            Metadata = metadata;
            ErrorCode = errorCode;
        }

        public EventDeserializationException(string eventTypeName, string metadata, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            EventTypeName = eventTypeName;
            Metadata = metadata;
            ErrorCode = errorCode;
        }
    }
}
