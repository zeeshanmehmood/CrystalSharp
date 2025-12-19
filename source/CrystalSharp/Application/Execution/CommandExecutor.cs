using CrystalSharp.Common.Settings;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Envoy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Execution
{
    public class CommandExecutor(IEnvoy envoy) : ICommandExecutor
    {
        private readonly IEnvoy _envoy = envoy;

        public async Task<CommandExecutionResult<TResult>> Execute<TResult>(ICommand<CommandExecutionResult<TResult>> command, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _envoy.Send(command, cancellationToken).ConfigureAwait(false);
            }
            catch (DomainException exception)
            {
                return CommandExecutionResult<TResult>.WithError([new(exception.ErrorCode, exception.Message)]);
            }
            catch (Exception exception)
            {
                return CommandExecutionResult<TResult>.WithError([new(ReservedErrorCode.SystemError, exception.Message)]);
            }
        }
    }
}
