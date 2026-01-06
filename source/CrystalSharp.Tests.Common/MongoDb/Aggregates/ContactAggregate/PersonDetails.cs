using CrystalSharp.Domain;
using System;
using System.Collections.Generic;

namespace CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate
{
    public class PersonDetails(string firstName, string lastName) : ValueObject
    {
        public string FirstName { get; private set; } = firstName;
        public string LastName { get; private set; } = lastName;

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
        }
    }
}
