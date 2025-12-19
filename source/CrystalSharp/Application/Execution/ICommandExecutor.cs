using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Application.Execution
{
    public interface ICommandExecutor
    {
        Task<CommandExecutionResult<TResult>> Execute<TResult>(ICommand<CommandExecutionResult<TResult>> command, CancellationToken cancellationToken = default);
    }
}
