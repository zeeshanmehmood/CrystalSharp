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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Application;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Sagas;

namespace CrystalSharp.Tests.Common.Sagas.Orchestration.TripOrchestration
{
    public class TripSaga : SagaOrchestrator<TripSagaLocator, PlanTripTransaction>
    {
        public TripSaga(IResolver resolver,
            ISagaStore sagaStore,
            TripSagaLocator sagaLocator)
            : base(resolver, sagaStore, sagaLocator)
        {
            //
        }

        public override async Task<SagaTransactionResult> Handle(PlanTripTransaction request, CancellationToken cancellationToken = default)
        {
            SagaResult sagaResult = await PrepareOrchestrator(request)
                .Activity<CreateTripActivity>("Create Trip")
                .Activity<BookHotelActivity>("Book Hotel")
                .WithCompensation<CancelHotelReservationActivity>("Cancel Hotel Reservation")
                .Activity<ReserveCarActivity>("Reserve Car")
                .WithCompensation<CancelCarActivity>("Cancel Car Reservation")
                .Activity<BookFlightActivity>("Book Flight")
                .WithCompensation<CancelFlightActivity>("Cancel Flight")
                .Activity<ConfirmTripActivity>("Confirm Trip")
                .WithCompensation<CancelTripActivity>("Cancel Trip")
                .Run(cancellationToken)
                .ConfigureAwait(false);

            IEnumerable<Error> errors = sagaResult.Trail.Where(t => t.Errors.HasAny()).SelectMany(e => e.Errors);

            return new SagaTransactionResult(sagaResult.CorrelationId, sagaResult.Success, errors);
        }
    }
}
