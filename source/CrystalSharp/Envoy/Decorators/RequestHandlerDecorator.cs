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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CrystalSharp.Envoy.Contracts;

namespace CrystalSharp.Envoy.Decorators
{
    public abstract class RequestHandlerDecorator
    {
        //
    }

    public abstract class RequestHandlerDecorator<TResponse> : RequestHandlerDecorator
    {
        public abstract Task<TResponse> Handle(IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default);
    }

    public class RequestHandlerDecorator<TRequest, TResponse> : RequestHandlerDecorator<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override async Task<TResponse> Handle(IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            IRequestHandler<TRequest, TResponse> handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            TResponse response = await handler.Handle((TRequest)request, cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}
