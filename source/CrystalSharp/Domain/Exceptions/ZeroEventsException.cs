using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Domain.Exceptions
{
    public class ZeroEventsException : Exception
    {
        public readonly int ErrorCode;

        public ZeroEventsException()
            : base("There are no events to store.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ZeroEventsException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ZeroEventsException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ZeroEventsException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
