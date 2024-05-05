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
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using CrystalSharp.Domain.Exceptions;
using CrystalSharp.Domain.Infrastructure;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate;
using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate.Events;

namespace CrystalSharp.Tests.UnitTests.Aggregates
{
    [Trait(TestSettings.Category, TestType.Unit)]
    public class AggregateRootTests
    {
        [Fact]
        public void Name_and_code_are_equal()
        {
            // Arrange
            string name = "Pakistani rupee";
            CurrencyInfo currencyInfo = new("PKR", 586);

            // Act
            Currency sut = Currency.Create(name, currencyInfo);

            // Assert
            using (new AssertionScope())
            {
                sut.Name.Should().Be("Pakistani rupee");
                sut.CurrencyInfo.Code.Should().Be("PKR");
                sut.CurrencyInfo.NumericCode.Should().Be(586);
            }
        }

        [Fact]
        public void Contains_one_event()
        {
            // Arrange
            string name = "Euro";
            CurrencyInfo currencyInfo = new("EUR", 978);
            Currency sut = Currency.Create(name, currencyInfo);

            // Act
            int result = sut.UncommittedEvents().Count;

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public void Contains_created_event()
        {
            // Arrange
            string name = "United States dollar";
            CurrencyInfo currencyInfo = new("USD", 840);
            Currency sut = Currency.Create(name, currencyInfo);

            // Act
            IDomainEvent result = sut.UncommittedEvents().SingleOrDefault(x => x.GetType() == typeof(CurrencyCreatedDomainEvent));

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void New_name_code_and_symbol_are_equal()
        {
            // Arrange
            string name = "Hungarian forint";
            string newName = "Euro";
            CurrencyInfo currencyInfo = new("HUF", 348);
            CurrencyInfo newCurrencyInfo = new("EUR", 978);
            Currency sut = Currency.Create(name, currencyInfo);

            // Act
            sut.ChangeName(newName);
            sut.ChangeInfo(newCurrencyInfo);

            // Assert
            using (new AssertionScope())
            {
                sut.Name.Should().Be("Euro");
                sut.CurrencyInfo.Code.Should().Be("EUR");
                sut.CurrencyInfo.NumericCode.Should().Be(978);
            }
        }

        [Fact]
        public void Contains_three_events()
        {
            // Arrange
            string name = "Turkish lira";
            string newName = "Euro";
            CurrencyInfo currencyInfo = new ("TRY", 949);
            CurrencyInfo newCurrencyInfo = new("EUR", 978);
            Currency sut = Currency.Create(name, currencyInfo);

            // Act
            sut.ChangeName(newName);
            sut.ChangeInfo(newCurrencyInfo);
            int result = sut.UncommittedEvents().Count;

            // Assert
            result.Should().Be(3);
        }

        [Fact]
        public void Contains_name_changed_event()
        {
            // Arrange
            string name = "European Euro";
            string newName = "Euro";
            CurrencyInfo currencyInfo = new("EUR", 978);
            Currency sut = Currency.Create(name, currencyInfo);

            // Act
            sut.ChangeName(newName);
            IDomainEvent result = sut.UncommittedEvents().SingleOrDefault(x => x.GetType() == typeof(CurrencyNameChangedDomainEvent));

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void Contains_four_events()
        {
            // Arrange
            string name = "Hungarian forint";
            string newName = "Euro";
            CurrencyInfo currencyInfo = new("HUF", 348);
            CurrencyInfo newCurrencyInfo = new("EUR", 978);

            // Act
            Currency sut = Currency.Create(name, currencyInfo);
            sut.ChangeName(newName);
            sut.ChangeInfo(newCurrencyInfo);
            sut.Delete();
            int result = sut.UncommittedEvents().Count;

            // Assert
            result.Should().Be(4);
        }

        [Fact]
        public void Contains_deleted_event()
        {
            // Arrange
            string name = "Hungarian forint";
            string newName = "Latvian lats";
            CurrencyInfo currencyInfo = new("HUF", 348);
            CurrencyInfo newCurrencyInfo = new("LVL", 111);
            Currency sut = Currency.Create(name, currencyInfo);
            sut.ChangeName(newName);
            sut.ChangeInfo(newCurrencyInfo);

            // Act
            sut.Delete();
            IDomainEvent result = sut.UncommittedEvents().SingleOrDefault(x => x.GetType() == typeof(CurrencyDeletedDomainEvent));

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void Contains_zero_event()
        {
            // Arrange
            string name = "Latvian lats";
            string newName = "Euro";
            CurrencyInfo currencyInfo = new("LVL", 111);
            CurrencyInfo newCurrencyInfo = new("EUR", 978);
            Currency sut = Currency.Create(name, currencyInfo);
            sut.ChangeName(newName);
            sut.ChangeInfo(newCurrencyInfo);
            sut.Delete();

            // Act
            sut.MarkEventsAsCommitted();
            int result = sut.UncommittedEvents().Count;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void Raise_name_validation_exception()
        {
            // Arrange
            string name = string.Empty;
            CurrencyInfo currencyInfo = new("EUR", 978);

            // Act
            Action sut = () => Currency.Create(name, currencyInfo);

            // Assert
            sut.Should().Throw<DomainException>();
        }

        [Fact]
        public void Raise_currency_info_validation_exception()
        {
            // Arrange
            string name = "Euro";

            // Act
            Action sut = () => Currency.Create(name, null);

            // Assert
            sut.Should().Throw<DomainException>();
        }

        [Fact]
        public void Raise_code_validation_exception()
        {
            // Arrange
            string name = "Euro";
            CurrencyInfo currencyInfo = new(string.Empty, 978);

            // Act
            Action sut = () => Currency.Create(name, currencyInfo);

            // Assert
            sut.Should().Throw<DomainException>();
        }

        [Fact]
        public void Raise_code_length_validation_exception()
        {
            // Arrange
            string name = "Euro";
            CurrencyInfo currencyInfo = new("EuroEUR", 978);

            // Act
            Action sut = () => Currency.Create(name, currencyInfo);

            // Assert
            sut.Should().Throw<DomainException>();
        }

        [Fact]
        public void Raise_numeric_code_validation_exception()
        {
            // Arrange
            string name = "Euro";
            CurrencyInfo currencyInfo = new("EUR", -1);

            // Act
            Action sut = () => Currency.Create(name, currencyInfo);

            // Assert
            sut.Should().Throw<DomainException>();
        }

        [Fact]
        public void Raise_numeric_code_length_validation_exception()
        {
            // Arrange
            string name = "Euro";
            CurrencyInfo currencyInfo = new("Euro", 0978);

            // Act
            Action sut = () => Currency.Create(name, currencyInfo);

            // Assert
            sut.Should().Throw<DomainException>();
        }
    }
}
