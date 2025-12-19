using System;

namespace CrystalSharp.Tests.Common.Application.CommandExecution.Responses
{
    public class CreateOrderResponse
    {
        public Guid Id { get; set; }
        public bool Success { get; set; } = false;
        public string OrderCode { get; set; }
    }
}
