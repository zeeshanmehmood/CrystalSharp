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
using Newtonsoft.Json;
using CrystalSharp.Domain.Infrastructure;

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

        public OrderPlacedDomainEvent(Guid streamId,
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
        public OrderPlacedDomainEvent(Guid streamId,
            string product,
            int quantity,
            decimal unitPrice,
            decimal amount,
            decimal amountPaid,
            bool paymentTransferred,
            bool delivered,
            int entityStatus,
            DateTime createdOn,
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
            CreatedOn = createdOn;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
