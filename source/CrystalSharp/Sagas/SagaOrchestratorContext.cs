using System;

namespace CrystalSharp.Sagas
{
    public class SagaOrchestratorContext(Guid correlationId, object data)
    {
        public Guid CorrelationId { get; } = correlationId;
        public object Data { get; } = data;
    }
}
