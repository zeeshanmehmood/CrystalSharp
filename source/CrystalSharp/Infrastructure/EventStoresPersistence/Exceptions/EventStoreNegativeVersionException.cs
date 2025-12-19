using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreNegativeVersionException : Exception
    {
        public readonly string Stream;
        public readonly long Version;
        public readonly int ErrorCode;

        public EventStoreNegativeVersionException()
            : base("The version cannot be negative.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreNegativeVersionException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreNegativeVersionException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreNegativeVersionException(string stream, long version, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            Version = version;
            ErrorCode = errorCode;
        }

        public EventStoreNegativeVersionException(string stream, long version, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            Version = version;
            ErrorCode = errorCode;
        }
    }
}
