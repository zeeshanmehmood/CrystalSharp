using CrystalSharp.Application;
using CrystalSharp.Application.Handlers;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Tests.Common.Application.CommandExecution.Commands;
using CrystalSharp.Tests.Common.Application.CommandExecution.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Application.CommandExecution.CommandHandlers
{
    public class CreateOrderCommandHandler : CommandHandler<CreateOrderCommand, CreateOrderResponse>
    {
        public override async Task<CommandExecutionResult<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken = default)
        {
            CreateOrderResponse response = new() { Id = Guid.Create(), Success = true, OrderCode = request.OrderCode };

            return await Ok(response);
        }
    }
}
