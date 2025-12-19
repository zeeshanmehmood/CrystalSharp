using CrystalSharp.Envoy.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    public class RequestHandlerDecorator<TRequest, TResponse>
        : RequestHandlerDecorator<TResponse>
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
