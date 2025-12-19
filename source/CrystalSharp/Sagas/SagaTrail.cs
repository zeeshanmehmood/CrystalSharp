using CrystalSharp.Application;
using System.Collections.Generic;

namespace CrystalSharp.Sagas
{
    public class SagaTrail(string step, bool success, IEnumerable<Error> errors)
    {
        public string Step { get; } = step;
        public bool Success { get; } = success;
        public IEnumerable<Error> Errors { get; } = errors;
    }
}
