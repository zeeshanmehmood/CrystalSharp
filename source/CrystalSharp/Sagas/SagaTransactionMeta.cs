using System;

namespace CrystalSharp.Sagas
{
    public class SagaTransactionMeta
    {
        public string Id { get; private set; } = Guid.CreateVersion7().ToString("N");
        public string CorrelationId { get; set; }
        public string StartedBy { get; set; }
        public string Step { get; set; }
        public SagaState State { get; set; }
        public string ErrorTrail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
