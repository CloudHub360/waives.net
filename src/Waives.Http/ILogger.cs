namespace Waives.Http
{
    public interface ILogger
    {
        void Log(LogLevel waivesLogLevel, string message);
    }
}