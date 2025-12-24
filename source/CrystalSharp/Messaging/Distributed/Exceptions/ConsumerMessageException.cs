using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class ConsumerMessageException : Exception
    {
        public readonly int ErrorCode;

        public ConsumerMessageException()
            : base("An error occurred when consuming messages from the message broker.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumerMessageException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumerMessageException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ConsumerMessageException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
