using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate.Events
{
    public class OrderPlacedDomainEvent : DomainEvent
    {
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public bool PaymentTransferred { get; set; }
        public bool Delivered { get; set; }

        public OrderPlacedDomainEvent(
            Guid streamId,
            string product,
            int quantity,
            decimal unitPrice,
            decimal amount,
            decimal amountPaid,
            bool paymentTransferred,
            bool delivered)
        {
            StreamId = streamId;
            Product = product;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Amount = amount;
            AmountPaid = amountPaid;
            PaymentTransferred = paymentTransferred;
            Delivered = delivered;
        }

        [JsonConstructor]
        public OrderPlacedDomainEvent(
            Guid streamId,
            string product,
            int quantity,
            decimal unitPrice,
            decimal amount,
            decimal amountPaid,
            bool paymentTransferred,
            bool delivered,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            Product = product;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Amount = amount;
            AmountPaid = amountPaid;
            PaymentTransferred = paymentTransferred;
            Delivered = delivered;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
