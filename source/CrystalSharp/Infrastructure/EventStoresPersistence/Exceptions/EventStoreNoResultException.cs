using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions
{
    public class EventStoreNoResultException : Exception
    {
        public readonly string Stream;
        public readonly int ErrorCode;

        public EventStoreNoResultException()
            : base("No result.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreNoResultException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public EventStoreNoResultException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventStoreNoResultException(string stream, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }

        public EventStoreNoResultException(string stream, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            ErrorCode = errorCode;
        }
    }
}
