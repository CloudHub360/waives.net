using System;

namespace Waives.Pipelines
{
    /// <summary>
    /// Indicates an unrecoverable error occurred in the processing of the pipeline.
    /// </summary>
    public class PipelineException : Exception
    {
        public PipelineException()
        {
        }

        public PipelineException(string message) : base(message)
        {
        }

        public PipelineException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}