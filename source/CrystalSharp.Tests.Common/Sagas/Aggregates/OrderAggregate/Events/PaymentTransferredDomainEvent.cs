using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate.Events
{
    public class PaymentTransferredDomainEvent : DomainEvent
    {
        public bool PaymentTransferred { get; set; }

        public PaymentTransferredDomainEvent(Guid streamId, bool paymentTransferred)
        {
            StreamId = streamId;
            PaymentTransferred = paymentTransferred;
        }

        [JsonConstructor]
        public PaymentTransferredDomainEvent(
            Guid streamId,
            bool paymentTransferred,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            PaymentTransferred = paymentTransferred;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
