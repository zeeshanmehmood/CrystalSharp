using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class ConsumingChannelIsNullException : Exception
    {
        public readonly int ErrorCode;

        public ConsumingChannelIsNullException()
            : base("Consuming channel is null.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumingChannelIsNullException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ConsumingChannelIsNullException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ConsumingChannelIsNullException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
