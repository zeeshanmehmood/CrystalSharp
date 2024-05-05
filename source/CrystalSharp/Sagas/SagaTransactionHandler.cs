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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Application;
using CrystalSharp.Application.Handlers;
using CrystalSharp.Common.Settings;
using CrystalSharp.Envoy;
using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Sagas
{
    public abstract class SagaTransactionHandler<TRequest> : Handler, IRequestHandler<TRequest, SagaTransactionResult>
        where TRequest : IRequest<SagaTransactionResult>
    {
        public abstract Task<SagaTransactionResult> Handle(TRequest request, CancellationToken cancellationToken = default);

        protected Task<SagaTransactionResult> Ok(Guid correlationId)
        {
            SagaTransactionResult result = new(correlationId, true);

            return Task.FromResult(result);
        }

        protected Task<SagaTransactionResult> Fail(Guid correlationId, params string[] errorMessages)
        {
            IEnumerable<Error> errors = errorMessages.ToList().Select(x => new Error(ReservedErrorCode.SystemError, x));
            SagaTransactionResult result = new(correlationId, false, errors);

            return Task.FromResult(result);
        }
    }
}
