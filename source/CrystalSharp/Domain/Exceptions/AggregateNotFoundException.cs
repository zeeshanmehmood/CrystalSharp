using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Domain.Exceptions
{
    public class AggregateNotFoundException : Exception
    {
        public readonly int ErrorCode;

        public AggregateNotFoundException()
            : base("Aggregate not found.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public AggregateNotFoundException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public AggregateNotFoundException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public AggregateNotFoundException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
