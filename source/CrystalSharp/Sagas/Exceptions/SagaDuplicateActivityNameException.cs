using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Sagas.Exceptions
{
    public class SagaDuplicateActivityNameException : Exception
    {
        public readonly int ErrorCode;

        public SagaDuplicateActivityNameException()
            : base("Duplicate activity name.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SagaDuplicateActivityNameException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SagaDuplicateActivityNameException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SagaDuplicateActivityNameException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
