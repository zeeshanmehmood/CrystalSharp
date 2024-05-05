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

namespace CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots.Exceptions
{
    public class SnapshotFrequencyException : Exception
    {
        public readonly string Stream;
        public readonly int Frequency;
        public readonly int ErrorCode;

        public SnapshotFrequencyException()
            : base("The snapshot frequency must be greater than zero (\"0\") and cannot be negative.")
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotFrequencyException(string message)
            : base(message)
        {
            ErrorCode = ReservedErrorCode.SystemError;
        }

        public SnapshotFrequencyException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public SnapshotFrequencyException(string stream, int frequency, int errorCode, string message)
            : base(message)
        {
            Stream = stream;
            Frequency = frequency;
            ErrorCode = errorCode;
        }

        public SnapshotFrequencyException(string stream, int frequency, int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            Stream = stream;
            Frequency = frequency;
            ErrorCode = errorCode;
        }
    }
}
