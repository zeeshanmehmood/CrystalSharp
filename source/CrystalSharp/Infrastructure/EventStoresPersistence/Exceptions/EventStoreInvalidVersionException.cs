using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreInvalidVersionException : Exception
    {
        public readonly string Stream;
        public readonly long Version;
        public readonly int ErrorCode;

        public EventStoreInvalidVersionException()
            : base("Invalid stream version.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreInvalidVersionException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreInvalidVersionException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreInvalidVersionException(string stream, long version, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            Version = version;
            ErrorCode = errorCode;
        }

        public EventStoreInvalidVersionException(string stream, long version, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            Version = version;
            ErrorCode = errorCode;
        }
    }
}
