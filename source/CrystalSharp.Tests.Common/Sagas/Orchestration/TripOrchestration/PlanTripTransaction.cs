using CrystalSharp.Sagas;

namespace CrystalSharp.Tests.Common.Sagas.Orchestration.TripOrchestration
{
    public class PlanTripTransaction : ISagaTransaction
    {
        public string Name { get; set; }
        public string Hotel { get; set; }
        public decimal ReservationAmount { get; set; }
        public decimal HotelReservationPaidByCustomer { get; set; }
        public string Car { get; set; }
        public decimal Rent { get; set; }
        public decimal CarRentPaidByCustomer { get; set; }
        public string Flight { get; set; }
        public decimal Fare { get; set; }
        public decimal FlightFarePaidByCustomer { get; set; }
    }
}
