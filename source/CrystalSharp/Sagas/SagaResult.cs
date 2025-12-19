using System;
using System.Collections.Generic;

namespace CrystalSharp.Sagas
{
    public class SagaResult(Guid correlationId, bool success, IEnumerable<SagaTrail> trail)
    {
        public Guid CorrelationId { get; } = correlationId;
        public bool Success { get; } = success;
        public IEnumerable<SagaTrail> Trail { get; } = trail;
    }
}
