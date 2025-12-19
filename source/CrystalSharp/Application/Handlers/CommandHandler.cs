using CrystalSharp.Common.Settings;
using CrystalSharp.Envoy;
using CrystalSharp.Envoy.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Handlers
{
    public abstract class CommandHandler<TRequest, TResponse> : Handler, IRequestHandler<TRequest, CommandExecutionResult<TResponse>>
        where TRequest : IRequest<CommandExecutionResult<TResponse>>
    {
        public abstract Task<CommandExecutionResult<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default);

        protected Task<CommandExecutionResult<TResponse>> Ok(TResponse data)
        {
            CommandExecutionResult<TResponse> result = new() { Success = true, Data = data };

            return Task.FromResult(result);
        }

        protected Task<CommandExecutionResult<TResponse>> Fail(params string[] errorMessages)
        {
            CommandExecutionResult<TResponse> result = new()
            {
                Success = false,
                Errors = errorMessages.ToList().Select(x => new Error(ReservedErrorCode.SystemError, x))
            };

            return Task.FromResult(result);
        }
    }
}
