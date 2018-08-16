using System;

namespace Waives.Pipelines
{
    internal class ProcessingError
    {
        public WaivesDocument Document { get; }
        public Exception Exception { get; }

        public ProcessingError(WaivesDocument document, Exception exception)
        {
            Document = document;
            Exception = exception;
        }
    }

    internal class ProcessingErrorDocument
    {
        public Document Document { get; }
        public Exception Exception { get; }

        public ProcessingErrorDocument(Document document, Exception exception)
        {
            Document = document;
            Exception = exception;
        }
    }
}