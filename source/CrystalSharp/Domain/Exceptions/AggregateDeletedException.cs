using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Domain.Exceptions
{
    public class AggregateDeletedException : Exception
    {
        public readonly int ErrorCode;

        public AggregateDeletedException()
            : base("Aggregate deleted.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public AggregateDeletedException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public AggregateDeletedException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public AggregateDeletedException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
