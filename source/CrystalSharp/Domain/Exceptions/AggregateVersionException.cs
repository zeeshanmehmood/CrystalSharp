using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.Domain.Exceptions
{
    public class AggregateVersionException : Exception
    {
        public readonly long AggregateVersion;
        public readonly long RequestedVersion;
        public readonly int ErrorCode;

        public AggregateVersionException()
            : base("Invalid aggregate version.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public AggregateVersionException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public AggregateVersionException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public AggregateVersionException(long aggregateVersion, long requestedVersion, int errorCode, string message)
            : base(message)
        {
            AggregateVersion = aggregateVersion;
            RequestedVersion = requestedVersion;
            ErrorCode = errorCode;
        }

        public AggregateVersionException(long aggregateVersion, long requestedVersion, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            AggregateVersion = aggregateVersion;
            RequestedVersion = requestedVersion;
            ErrorCode = errorCode;
        }
    }
}
