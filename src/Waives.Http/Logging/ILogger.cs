namespace Waives.Http.Logging
{
    public interface ILogger
    {
        void Log(LogLevel logLevel, string message);
    }
}