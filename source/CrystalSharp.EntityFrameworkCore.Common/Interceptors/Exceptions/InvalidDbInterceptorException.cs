using CrystalSharp.Common.Settings;
using System;

namespace CrystalSharp.EntityFrameworkCore.Common.Interceptors.Exceptions
{
    public class InvalidDbInterceptorException : Exception
    {
        public readonly int ErrorCode;

        public InvalidDbInterceptorException()
            : base("Invalid DB interceptor.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public InvalidDbInterceptorException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public InvalidDbInterceptorException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public InvalidDbInterceptorException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
