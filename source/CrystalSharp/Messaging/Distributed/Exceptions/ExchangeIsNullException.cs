using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class ExchangeIsNullException : Exception
    {
        public readonly int ErrorCode;

        public ExchangeIsNullException()
            : base("Exchange is null.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ExchangeIsNullException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ExchangeIsNullException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ExchangeIsNullException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
