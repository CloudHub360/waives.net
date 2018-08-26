﻿using System;

namespace Waives.Http.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(LogLevel logLevel, string message)
        {
            if (logLevel < LogLevel.Info)
            {
                return;
            }

            Console.WriteLine($"[{logLevel}] {message}");
        }
    }
}