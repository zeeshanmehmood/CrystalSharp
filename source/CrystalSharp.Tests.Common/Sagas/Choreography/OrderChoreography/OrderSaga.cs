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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrystalSharp.Domain;
using CrystalSharp.Sagas;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate.Events;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;

namespace CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography
{
    public class OrderSaga : SagaChoreography<OrderSagaLocator, PlaceOrderTransaction>,
        IAmStartedBy<PlaceOrderTransaction>,
        ISagaChoreographyEvent<Order, int, OrderPlacedDomainEvent>,
        ISagaChoreographyEvent<Order, int, PaymentTransferredDomainEvent>,
        ISagaChoreographyEvent<Order, int, OrderDeliveredDomainEvent>
    {
        private readonly IInMemoryDataContext _dataContext;

        public OrderSaga(IInMemoryDataContext dataContext,
            ISagaStore sagaStore,
            OrderSagaLocator sagaLocator,
            ISagaTransactionExecutor sagaTransactionExecutor)
            : base(sagaStore, sagaLocator, sagaTransactionExecutor)
        {
            _dataContext = dataContext;
        }

        public override async Task<SagaTransactionResult> Handle(PlaceOrderTransaction request, CancellationToken cancellationToken = default)
        {
            Order order = Order.PlaceOrder(request.Product, request.Quantity, request.UnitPrice, request.AmountPaid);

            await _dataContext.Order.AddAsync(order, cancellationToken).ConfigureAwait(false);
            await _dataContext.SaveChanges(cancellationToken).ConfigureAwait(false);

            return await Ok(order.GlobalUId);
        }

        public async Task Handle(OrderPlacedDomainEvent @event, CancellationToken cancellationToken = default)
        {
            TransferPaymentTransaction transaction = new() { GlobalUId = @event.StreamId };

            async Task compensation() { await RejectOrder(@event.StreamId, cancellationToken).ConfigureAwait(false); }

            await Execute(@event.StreamId, transaction, compensation, cancellationToken).ConfigureAwait(false);
        }

        public async Task Handle(PaymentTransferredDomainEvent @event, CancellationToken cancellationToken = default)
        {
            if (@event.PaymentTransferred)
            {
                DeliverOrderTransaction transaction = new() { GlobalUId = @event.StreamId };

                await Execute(@event.StreamId, transaction, null, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Handle(OrderDeliveredDomainEvent @event, CancellationToken cancellationToken = default)
        {
            await MarkAsComplete(@event.StreamId, cancellationToken).ConfigureAwait(false);
        }

        private async Task RejectOrder(Guid globalUId, CancellationToken cancellationToken)
        {
            Order order = await _dataContext.Order.SingleOrDefaultAsync(x => 
                x.EntityStatus == EntityStatus.Active 
                && x.GlobalUId == globalUId,
                cancellationToken)
                .ConfigureAwait(false);

            if (order != null)
            {
                order.Delete();

                await _dataContext.SaveChanges(cancellationToken);
            }
        }
    }
}
