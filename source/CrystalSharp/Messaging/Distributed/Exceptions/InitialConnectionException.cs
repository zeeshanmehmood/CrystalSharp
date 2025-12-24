using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Messaging.Distributed.Exceptions
{
    public class InitialConnectionException : Exception
    {
        public readonly int ErrorCode;

        public InitialConnectionException()
            : base("Could not establish an initial connection.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public InitialConnectionException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public InitialConnectionException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public InitialConnectionException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
