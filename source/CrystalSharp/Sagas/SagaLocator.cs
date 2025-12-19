using CrystalSharp.Common.Settings;
using System;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public abstract class SagaLocator : ISagaLocator
    {
        public virtual async Task<string> Locate(Guid correlationId)
        {
            string sagaId = $"{Prefix.SagaIdPrefix}{correlationId:N}";

            return await Task.FromResult(sagaId);
        }
    }
}
