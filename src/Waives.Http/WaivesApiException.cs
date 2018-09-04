using System;

namespace Waives.Http
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an error occurring in calling the Waives platform API
    /// </summary>
    public class WaivesApiException : Exception
    {
        /// <inheritdoc />
        public WaivesApiException()
        {
        }

        /// <inheritdoc />
        public WaivesApiException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public WaivesApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}