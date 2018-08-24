namespace Waives.Http.Logging
{
    public interface ILogger
    {
        void Log(LogLevel waivesLogLevel, string message);
    }
}