using System;

namespace Waives.Pipelines
{
    public class DocumentError
    {
        public Document Document { get; }
        public Exception Exception { get; }

        public DocumentError(Document document, Exception exception)
        {
            Document = document;
            Exception = exception;
        }
    }
}