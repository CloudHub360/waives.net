using System;

namespace Waives.Pipelines
{
    /// <summary>
    /// Indicates an unrecoverable error occurred in the processing of the pipeline.
    /// </summary>
    public class PipelineException : Exception
    {
        // ReSharper disable once UnusedMember.Global
        internal PipelineException()
        {
        }

        // ReSharper disable once UnusedMember.Global
        internal PipelineException(string message) : base(message)
        {
        }

        // ReSharper disable once UnusedMember.Global
        internal PipelineException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}