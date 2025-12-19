namespace CrystalSharp.Sagas
{
    public enum SagaState
    {
        New = 0,
        Committed = 1,
        Active = 2,
        Aborted = 3
    }
}
