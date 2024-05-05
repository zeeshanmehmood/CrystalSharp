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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Domain;
using CrystalSharp.Sagas;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;

namespace CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.TransactionHandlers
{
    public class DeliverOrderTransactionHandler : SagaTransactionHandler<DeliverOrderTransaction>
    {
        private readonly IInMemoryDataContext _dataContext;

        public DeliverOrderTransactionHandler(IInMemoryDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public override async Task<SagaTransactionResult> Handle(DeliverOrderTransaction request, CancellationToken cancellationToken = default)
        {
            Order order = _dataContext.Order.SingleOrDefault(x => x.EntityStatus == EntityStatus.Active && x.GlobalUId == request.GlobalUId);

            if (order == null)
            {
                return await Fail(request.GlobalUId, "Order not found.");
            }

            order.Deliver();
            await _dataContext.SaveChanges(cancellationToken).ConfigureAwait(false);

            return await Ok(order.GlobalUId);
        }
    }
}
