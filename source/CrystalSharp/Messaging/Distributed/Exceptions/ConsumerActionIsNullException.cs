using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class ConsumerActionIsNullException : Exception
    {
        public readonly int ErrorCode;

        public ConsumerActionIsNullException()
            : base("Consumer action is null.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumerActionIsNullException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumerActionIsNullException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ConsumerActionIsNullException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
