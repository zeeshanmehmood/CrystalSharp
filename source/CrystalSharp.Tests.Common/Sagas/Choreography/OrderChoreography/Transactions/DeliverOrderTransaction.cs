using CrystalSharp.Sagas;
using System;

namespace CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions
{
    public class DeliverOrderTransaction : ISagaTransaction
    {
        public Guid GlobalUId { get; set; }
    }
}
