namespace CrystalSharp.Application
{
    public class Error(int code, string message)
    {
        public int Code { get; } = code;
        public string Message { get; } = message;
    }
}
