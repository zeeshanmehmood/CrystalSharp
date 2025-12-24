using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class ProducingChannelIsNullException : Exception
    {
        public readonly int ErrorCode;

        public ProducingChannelIsNullException()
            : base("Producing channel is null.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ProducingChannelIsNullException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public ProducingChannelIsNullException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ProducingChannelIsNullException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
