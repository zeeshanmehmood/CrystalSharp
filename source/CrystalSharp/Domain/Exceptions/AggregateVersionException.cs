// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using CrystalSharp.Common.Settings;

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
