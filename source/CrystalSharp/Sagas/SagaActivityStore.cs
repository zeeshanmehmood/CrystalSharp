using System;

namespace CrystalSharp.Sagas
{
    public class SagaActivityStore
    {
        public string ActivityName { get; set; }
        public Type ActivityType { get; set; }
        public string CompensationName { get; set; }
        public Type CompensationType { get; set; }
    }
}
