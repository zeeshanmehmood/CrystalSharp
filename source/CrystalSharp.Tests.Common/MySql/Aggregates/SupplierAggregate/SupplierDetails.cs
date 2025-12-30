using CrystalSharp.Domain;
using System;
using System.Collections.Generic;

namespace CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate
{
    public class SupplierDetails(string code, string email) : ValueObject
    {
        public string Code { get; private set; } = code;
        public string Email { get; private set; } = email;

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Code;
            yield return Email;
        }
    }
}
