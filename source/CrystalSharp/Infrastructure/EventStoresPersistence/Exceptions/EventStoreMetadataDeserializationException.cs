using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreMetadataDeserializationException : Exception
    {
        public readonly string Metadata;
        public readonly int ErrorCode;

        public EventStoreMetadataDeserializationException()
            : base("The metadata cannot be deserialized.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreMetadataDeserializationException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreMetadataDeserializationException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreMetadataDeserializationException(string metadata, int errorCode, string message)
            : base(message)
        {
            Metadata = metadata;
            ErrorCode = errorCode;
        }

        public EventStoreMetadataDeserializationException(string metadata, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Metadata = metadata;
            ErrorCode = errorCode;
        }
    }
}
