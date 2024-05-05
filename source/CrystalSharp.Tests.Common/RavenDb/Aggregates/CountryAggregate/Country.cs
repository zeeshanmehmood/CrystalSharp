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
using CrystalSharp.Tests.Common.RavenDb.Aggregates.CountryAggregate.Events;

namespace CrystalSharp.Tests.Common.RavenDb.Aggregates.CountryAggregate
{
    public class Country : AggregateRoot<string>
    {
        public string Name { get; private set; }
        public CountryInfo CountryInfo { get; private set; }

        private static void ValidateCountry(Country country)
        {
            if (string.IsNullOrEmpty(country.Name))
            {
                country.ThrowDomainException("Country name is required.");
            }

            if (country.CountryInfo == null)
            {
                country.ThrowDomainException("Country info is required.");
            }

            if (string.IsNullOrEmpty(country.CountryInfo.Code))
            {
                country.ThrowDomainException("Country code is required.");
            }

            if (country.CountryInfo.Code.Length > 2)
            {
                country.ThrowDomainException("Only two characters allowed for code.");
            }

            if (country.CountryInfo.NumericCode <= 0)
            {
                country.ThrowDomainException("Country numeric code should be greater than 0.");
            }

            if (country.CountryInfo.NumericCode.ToString().Length > 3)
            {
                country.ThrowDomainException("Only three digits allowed for numeric code.");
            }
        }

        public static Country Create(string name, CountryInfo countryInfo)
        {
            Country country = new() { Name = name, CountryInfo = countryInfo };

            ValidateCountry(country);

            country.Raise(new CountryCreatedDomainEvent(country.GlobalUId, country.Name, country.CountryInfo));

            return country;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateCountry(this);

            Raise(new CountryNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeInfo(CountryInfo countryInfo)
        {
            CountryInfo = countryInfo;

            ValidateCountry(this);

            Raise(new CountryInfoChangedDomainEvent(GlobalUId, CountryInfo));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new CountryDeletedDomainEvent(GlobalUId, Name, CountryInfo));
        }

        private void Apply(CountryCreatedDomainEvent @event)
        {
            Name = @event.Name;
            CountryInfo = @event.CountryInfo;
        }

        private void Apply(CountryNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(CountryInfoChangedDomainEvent @event)
        {
            CountryInfo = @event.CountryInfo;
        }

        private void Apply(CountryDeletedDomainEvent @event)
        {
            //
        }
    }
}
