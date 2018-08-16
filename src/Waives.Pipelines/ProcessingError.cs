using System;

namespace Waives.Pipelines
{
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