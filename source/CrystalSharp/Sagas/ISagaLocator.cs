using System;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public interface ISagaLocator
    {
        Task<string> Locate(Guid correlationId);
    }
}
