﻿namespace Waives.Http.Logging
{

    internal class NoopLogger : ILogger
    {
        public void Log(LogLevel logLevel, string message)
        {
        }
    }
}