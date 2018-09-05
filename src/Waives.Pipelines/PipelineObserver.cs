using System;

namespace Waives.Pipelines
{
    internal class PipelineObserver : IObserver<WaivesDocument>
    {
        private readonly Action _onPipelineCompleted;

        internal PipelineObserver(Action onPipelineCompleted)
        {
            _onPipelineCompleted = onPipelineCompleted;
        }

        public void OnCompleted()
        {
            _onPipelineCompleted();
        }

        public void OnError(Exception error)
        {
            throw new PipelineException($"A fatal error occurred in the processing pipeline: {error.Message}", error);
        }

        public void OnNext(WaivesDocument value)
        {

        }
    }
}