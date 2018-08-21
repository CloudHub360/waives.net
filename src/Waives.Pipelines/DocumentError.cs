using System;

namespace Waives.Pipelines
{
    /// <summary>
    /// Represents an error that has occurred during processing of a
    /// <see cref="Document"/> through a <see cref="Pipeline"/>
    /// </summary>
    public class DocumentError
    {
        /// <summary>
        /// The <see cref="Document"/> on which the error occurred
        /// </summary>
        public Document Document { get; }
        /// <summary>
        /// The exception containing details of the error
        /// </summary>
        public Exception Exception { get; }

        public DocumentError(Document document, Exception exception)
        {
            Document = document;
            Exception = exception;
        }
    }
}