using CrystalSharp.Domain;
using System;
using System.Collections.Generic;

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate
{
    public class CurrencyDetails(string code, int numericCode) : ValueObject
    {
        public string Code { get; private set; } = code;
        public int NumericCode { get; private set; } = numericCode;

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Code;
            yield return NumericCode;
        }
    }
}
