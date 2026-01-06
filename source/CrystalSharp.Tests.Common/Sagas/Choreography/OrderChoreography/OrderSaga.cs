using CrystalSharp.Domain;
using CrystalSharp.Sagas;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate.Events;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography
{
    public class OrderSaga(
        IInMemoryDataContext dataContext,
        ISagaStore sagaStore,
        OrderSagaLocator sagaLocator,
        ISagaTransactionExecutor sagaTransactionExecutor)
        : SagaChoreography<OrderSagaLocator, PlaceOrderTransaction>(sagaStore, sagaLocator, sagaTransactionExecutor),
        IAmStartedBy<PlaceOrderTransaction>,
        ISagaChoreographyEvent<Order, int, OrderPlacedDomainEvent>,
        ISagaChoreographyEvent<Order, int, PaymentTransferredDomainEvent>,
        ISagaChoreographyEvent<Order, int, OrderDeliveredDomainEvent>
    {
        private readonly IInMemoryDataContext _dataContext = dataContext;

        public override async Task<SagaTransactionResult> Handle(PlaceOrderTransaction request, CancellationToken cancellationToken = default)
        {
            Order order = Order.PlaceOrder(request.Product, request.Quantity, request.UnitPrice, request.AmountPaid);

            await _dataContext.Order.AddAsync(order, cancellationToken).ConfigureAwait(false);
            await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

                await _dataContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
