using CrystalSharp.Domain;
using System;
using System.Collections.Generic;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate
{
    public class CourseInfo(int lectures, decimal fees) : ValueObject
    {
        public int Lectures { get; private set; } = lectures;
        public decimal Fees { get; private set; } = fees;

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Lectures;
            yield return Fees;
        }
    }
}
