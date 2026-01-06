using CrystalSharp.Domain;
using System;

namespace CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate
{
    public class Trip : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public string Hotel { get; private set; }
        public decimal HotelReservation { get; private set; }
        public decimal HotelReservationPaidByCustomer { get; private set; }
        public bool HotelReservationConfirmed { get; private set; } = false;
        public string Car { get; private set; }
        public decimal CarRent { get; private set; }
        public decimal CarRentPaidByCustomer { get; private set; }
        public bool CarReserved { get; private set; } = false;
        public string Flight { get; private set; }
        public decimal FlightFare { get; private set; }
        public decimal FlightFarePaidByCustomer { get; private set; }
        public bool FlightConfirmed { get; private set; } = false;
        public decimal TotalAmount { get; private set; }
        public bool Confirm { get; private set; } = false;
        public Guid CorrelationId { get; private set; }

        public static Trip Create(string name, Guid correlationId)
        {
            Trip trip = new() { Name = name, Confirm = false, CorrelationId = correlationId };

            return trip;
        }

        public void BookHotel(string hotel, decimal reservationAmount, decimal amountPaidByCutomer)
        {
            ValidateServiceAmount(reservationAmount, amountPaidByCutomer);

            Hotel = hotel;
            HotelReservation = reservationAmount;
            HotelReservationPaidByCustomer = amountPaidByCutomer;
            HotelReservationConfirmed = true;

            SetTotalAmount(HotelReservation);
        }

        public void CancelHotelReservation()
        {
            HotelReservationPaidByCustomer = 0;
            HotelReservationConfirmed = false;
        }

        public void ReserveCar(string car, decimal rent, decimal amountPaidByCustomer)
        {
            ValidateServiceAmount(rent, amountPaidByCustomer);

            Car = car;
            CarRent = rent;
            CarRentPaidByCustomer = amountPaidByCustomer;
            CarReserved = true;

            SetTotalAmount(CarRent);
        }

        public void CancelCar()
        {
            CarRentPaidByCustomer = 0;
            CarReserved = false;
        }

        public void BookFlight(string flight, decimal fare, decimal amountPaidByCustomer)
        {
            ValidateServiceAmount(fare, amountPaidByCustomer);

            Flight = flight;
            FlightFare = fare;
            FlightFarePaidByCustomer = amountPaidByCustomer;
            FlightConfirmed = true;

            SetTotalAmount(FlightFare);
        }

        public void CancelFlight()
        {
            FlightFarePaidByCustomer = 0;
            FlightConfirmed = false;
        }

        public void ConfirmTrip()
        {
            decimal amountPaid = HotelReservationPaidByCustomer + CarRentPaidByCustomer + FlightFarePaidByCustomer;

            ValidateTotalAmount(TotalAmount, amountPaid);

            Confirm = true;
        }

        public void CancelTrip()
        {
            CancelHotelReservation();
            CancelCar();
            CancelFlight();

            Confirm = false;
        }

        public override void Delete()
        {
            base.Delete();
        }

        private void SetTotalAmount(decimal amount)
        {
            TotalAmount += amount;
        }

        private void ValidateServiceAmount(decimal serviceAmount, decimal amountPaidByCustomer)
        {
            if (amountPaidByCustomer < serviceAmount)
            {
                ThrowDomainException("The paid amount is less than the amount required for this service.");
            }
        }

        private void ValidateTotalAmount(decimal totalAmount, decimal amountPaid)
        {
            if (amountPaid < totalAmount)
            {
                ThrowDomainException("The paid amount is less than the amount required for this trip.");
            }
        }
    }
}
