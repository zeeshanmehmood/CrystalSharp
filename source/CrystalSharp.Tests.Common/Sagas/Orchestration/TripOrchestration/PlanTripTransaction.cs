// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
