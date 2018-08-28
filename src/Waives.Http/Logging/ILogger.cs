namespace Waives.Http.Logging
{
    /// <summary>
    /// Defines operations for logging messages that are written by <see cref="WaivesClient"/>
    /// and Pipeline to your own logging framework.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="logLevel">The level of the message</param>
        /// <param name="message">The contents of the message</param>
        void Log(LogLevel logLevel, string message);
    }
}