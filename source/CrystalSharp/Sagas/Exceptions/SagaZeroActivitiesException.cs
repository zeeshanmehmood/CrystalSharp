using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Sagas.Exceptions
{
    public class SagaZeroActivitiesException : Exception
    {
        public readonly int ErrorCode;

        public SagaZeroActivitiesException()
            : base("There are no activities defined.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SagaZeroActivitiesException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SagaZeroActivitiesException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SagaZeroActivitiesException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
