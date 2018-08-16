using System;

namespace Waives.Reactive
{
    /// <summary>
    /// Indicates an unrecoverable error occurred in the processing of the pipeline.
    /// </summary>
    public class PipelineException : Exception
    {
        public PipelineException(Exception innerException)
            : base("A fatal error occurred in the processing pipeline. View the InnerException" +
                   "for more details.", innerException)
        {

        }
    }
}