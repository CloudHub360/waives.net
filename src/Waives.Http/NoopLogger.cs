namespace Waives.Http
{

    internal class NoopLogger : ILogger
    {
        public void Log(LogLevel waivesLogLevel, string message)
        {
        }
    }
}