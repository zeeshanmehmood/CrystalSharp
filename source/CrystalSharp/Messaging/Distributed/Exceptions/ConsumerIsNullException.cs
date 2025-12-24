using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class ConsumerIsNullException : Exception
    {
        public readonly int ErrorCode;

        public ConsumerIsNullException()
            : base("Consumer is null.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumerIsNullException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumerIsNullException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ConsumerIsNullException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
