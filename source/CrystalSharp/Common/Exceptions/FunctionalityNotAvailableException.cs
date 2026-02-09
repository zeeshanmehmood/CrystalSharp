using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Common.Exceptions
{
    public class FunctionalityNotAvailableException : Exception
    {
        public readonly int ErrorCode;

        public FunctionalityNotAvailableException()
            : base("This functionality is not available.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public FunctionalityNotAvailableException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public FunctionalityNotAvailableException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public FunctionalityNotAvailableException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
