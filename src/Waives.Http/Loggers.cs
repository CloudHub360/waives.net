namespace Waives.Http
{
    internal static class Loggers
    {
        internal static NoopLogger NoopLogger { get; private set; } = new NoopLogger();
    }
}