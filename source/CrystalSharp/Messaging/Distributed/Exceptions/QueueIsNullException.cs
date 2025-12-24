using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class QueueIsNullException : Exception
    {
        public readonly int ErrorCode;

        public QueueIsNullException()
            : base("Queue is null.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public QueueIsNullException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public QueueIsNullException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public QueueIsNullException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
