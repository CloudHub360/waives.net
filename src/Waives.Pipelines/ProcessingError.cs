using System;

namespace Waives.Pipelines
{
    /// <summary>
    /// Represents an error that occurred during processing, either on
    /// a WaivesDocument (during processing) or a Document (during creation).
    /// </summary>
    /// <typeparam name="T">The type of document on which the error occurred,
    /// either <see cref="WaivesDocument"/> or <see cref="Document"/></typeparam>
    internal class ProcessingError<T>
    {
        public T Document { get; }
        public Exception Exception { get; }

        public ProcessingError(T document, Exception exception)
        {
            Document = document;
            Exception = exception;
        }
    }
}