namespace Waives.Http.Logging
{
    /// <summary>
    /// The level of a log message, as supplied to <see cref="ILogger"/>.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Verbose events describe the lowest level of internal system events, and are potentially very noisy.
        /// </summary>
        Trace,
        /// <summary>
        /// Debug events describe internal system events that are not necessarily observable from the outside, but
        /// useful when determining how something happened.
        /// </summary>
        Debug,
        /// <summary>
        /// Information events describe things happening in the system that correspond to its responsibilities
        /// and functions. Generally these are the observable actions the system can perform.
        /// </summary>
        Info,
        /// <summary>
        /// When service is degraded, endangered, or may be behaving outside of its expected parameters,
        /// Warning level events are used.
        /// </summary>
        Warn,
        /// <summary>
        /// When functionality is unavailable or expectations broken, an Error event is used.
        /// </summary>
        Error,
        /// <summary>
        /// The most critical level, Fatal events demand immediate attention
        /// </summary>
        Fatal
    }
}