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
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Sagas;
using CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;

namespace CrystalSharp.Tests.Common.Sagas.Orchestration.TripOrchestration
{
    public class CancelHotelReservationActivity : ISagaActivity
    {
        private readonly IInMemoryDataContext _dataContext;

        public CancelHotelReservationActivity(IInMemoryDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<SagaTransactionResult> Execute(SagaOrchestratorContext sagaContext, CancellationToken cancellationToken = default)
        {
            PlanTripTransaction transaction = (PlanTripTransaction)sagaContext.Data;
            Trip trip = _dataContext.Trip.SingleOrDefault(x => x.EntityStatus == EntityStatus.Active && x.CorrelationId == sagaContext.CorrelationId);

            if (trip == null)
            {
                return SagaTransactionResult.WithError(new List<Error> { new Error(ReservedErrorCode.SystemError, "Trip not found.") });
            }

            trip.CancelHotelReservation();
            await _dataContext.SaveChanges(cancellationToken).ConfigureAwait(false);

            return new SagaTransactionResult(sagaContext.CorrelationId, true);
        }
    }
}
