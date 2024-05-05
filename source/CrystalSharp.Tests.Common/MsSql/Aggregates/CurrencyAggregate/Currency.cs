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

using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate.Events;

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate
{
    public class Currency : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public CurrencyInfo CurrencyInfo { get; private set; }

        private static void ValidateCurrency(Currency currency)
        {
            if (string.IsNullOrEmpty(currency.Name))
            {
                currency.ThrowDomainException("Currency name is required.");
            }

            if (currency.CurrencyInfo == null)
            {
                currency.ThrowDomainException("Currency info is required.");
            }

            if (string.IsNullOrEmpty(currency.CurrencyInfo.Code))
            {
                currency.ThrowDomainException("Currency code is required.");
            }

            if (currency.CurrencyInfo.Code.Length > 3)
            {
                currency.ThrowDomainException("Only three characters allowed for code.");
            }

            if (currency.CurrencyInfo.NumericCode <= 0)
            {
                currency.ThrowDomainException("Currency numeric code should be greater than 0.");
            }

            if (currency.CurrencyInfo.NumericCode.ToString().Length > 3)
            {
                currency.ThrowDomainException("Only three digits allowed for numeric code.");
            }
        }

        public static Currency Create(string name, CurrencyInfo currencyInfo)
        {
            Currency currency = new() { Name = name, CurrencyInfo = currencyInfo };

            ValidateCurrency(currency);

            currency.Raise(new CurrencyCreatedDomainEvent(currency.GlobalUId, currency.Name, currencyInfo));

            return currency;
        }

        public void ChangeName(string name)
        {
            Name = name;

            Raise(new CurrencyNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeInfo(CurrencyInfo currencyInfo)
        {
            CurrencyInfo = currencyInfo;

            ValidateCurrency(this);

            Raise(new CurrencyInfoChangedDomainEvent(GlobalUId, CurrencyInfo));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new CurrencyDeletedDomainEvent(GlobalUId, Name, CurrencyInfo));
        }

        private void Apply(CurrencyCreatedDomainEvent @event)
        {
            Name = @event.Name;
            CurrencyInfo = @event.CurrencyInfo;
        }

        private void Apply(CurrencyNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(CurrencyInfoChangedDomainEvent @event)
        {
            CurrencyInfo = @event.CurrencyInfo;
        }

        private void Apply(CurrencyDeletedDomainEvent @event)
        {
            Name = @event.Name;
            CurrencyInfo = @event.CurrencyInfo;
        }
    }
}
