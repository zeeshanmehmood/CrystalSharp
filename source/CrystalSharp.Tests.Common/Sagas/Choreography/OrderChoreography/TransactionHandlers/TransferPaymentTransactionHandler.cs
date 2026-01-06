using CrystalSharp.Domain;
using CrystalSharp.Sagas;
using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.Transactions;
using CrystalSharp.Tests.Common.Sagas.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Sagas.Choreography.OrderChoreography.TransactionHandlers
{
    public class TransferPaymentTransactionHandler(IInMemoryDataContext dataContext) : SagaTransactionHandler<TransferPaymentTransaction>
    {
        private readonly IInMemoryDataContext _dataContext = dataContext;

        public override async Task<SagaTransactionResult> Handle(TransferPaymentTransaction request, CancellationToken cancellationToken = default)
        {
            Order order = _dataContext.Order.SingleOrDefault(x => x.EntityStatus == EntityStatus.Active && x.GlobalUId == request.GlobalUId);

            if (order is null)
            {
                return await Fail(request.GlobalUId, "Order not found.");
            }

            order.TransferPayment();
            await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await Ok(order.GlobalUId);
        }
    }
}
