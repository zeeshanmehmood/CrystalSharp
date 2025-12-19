using CrystalSharp.Application;
using CrystalSharp.Tests.Common.Application.CommandExecution.Responses;

namespace CrystalSharp.Tests.Common.Application.CommandExecution.Commands
{
    public class CreateOrderCommand : ICommand<CommandExecutionResult<CreateOrderResponse>>
    {
        public string OrderCode { get; set; }
    }
}
