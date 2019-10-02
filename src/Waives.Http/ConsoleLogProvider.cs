using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Waives.Http.Logging;
using Waives.Http.Logging.LogProviders;

namespace Waives.Http
{
    /// <summary>
    /// ConsoleLogProvider is an example ILogProvider implementation. It is not recommended for use
    /// in production.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public sealed class ConsoleLogProvider : LogProviderBase
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>
        {
            {LogLevel.Fatal, ConsoleColor.Red},
            {LogLevel.Error, ConsoleColor.Yellow},
            {LogLevel.Warn, ConsoleColor.Magenta},
            {LogLevel.Info, ConsoleColor.White},
            {LogLevel.Debug, ConsoleColor.Gray},
            {LogLevel.Trace, ConsoleColor.DarkGray},
        };

        private static bool Log(LogLevel logLevel,
            Func<string> messageFunc,
            Exception exception = null,
            params object[] formatParameters)
        {
            if (messageFunc == null)
            {
                return true;
            }

            var message = ReplaceStructuredLoggingTokens(messageFunc(), formatParameters);

            Console.WriteLine($"[{DateTime.Now}] [{logLevel}] {message} {exception}");
            return true;
        }

        public override Logger GetLogger(string name)
        {
            return (logLevel, messageFunc, exception, formatParameters) =>
            {
                if (messageFunc == null)
                {
                    return true;
                }

                if (Colors.TryGetValue(logLevel, out ConsoleColor consoleColor))
                {
                    var originalForeground = Console.ForegroundColor;
                    try
                    {
                        Console.ForegroundColor = consoleColor;
                        Log(logLevel, messageFunc, exception, formatParameters);
                    }
                    finally
                    {
                        Console.ForegroundColor = originalForeground;
                    }
                }
                else
                {
                    Log(logLevel, messageFunc, exception, formatParameters);
                }

                return true;
            };
        }

        private static string ReplaceStructuredLoggingTokens(string message,
            params object[] formatParameters)
        {
            var tokens = Regex.Matches(message,
                @"{\w+}",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);
            var i = 0;

            foreach (Match match in tokens)
            {
                var param = i < formatParameters.Length ? formatParameters[i++].ToString() : "*NO PARAM*";

                message = message.Replace(match.ToString(), $"{{{param}}}");
            }

            return message;
        }
    }
}