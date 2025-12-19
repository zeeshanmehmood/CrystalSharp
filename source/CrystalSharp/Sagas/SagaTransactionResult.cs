using CrystalSharp.Application;
using System;
using System.Collections.Generic;

namespace CrystalSharp.Sagas
{
    public class SagaTransactionResult(Guid correlationId, bool success, IEnumerable<Error> errors = null)
    {
        public Guid CorrelationId { get; } = correlationId;
        public bool Success { get; } = success;
        public IEnumerable<Error> Errors { get; } = errors;

        public static SagaTransactionResult WithError(IEnumerable<Error> errors = null)
        {
            return new SagaTransactionResult(Guid.Empty, false, errors);
        }
    }
}
