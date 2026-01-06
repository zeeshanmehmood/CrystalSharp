using CrystalSharp.Sagas;

namespace CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions
{
    public class PlaceOrderTransaction : ISagaTransaction
    {
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal AmountPaid { get; set; }
    }
}
