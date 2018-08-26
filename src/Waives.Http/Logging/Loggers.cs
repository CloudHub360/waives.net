namespace Waives.Http.Logging
{
    internal static class Loggers
    {
        internal static NoopLogger NoopLogger { get; private set; } = new NoopLogger();
        internal static ConsoleLogger ConsoleLogger { get; private set; } = new ConsoleLogger();
    }
}