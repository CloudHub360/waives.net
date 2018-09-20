using System;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class ConcurrentPipelineObserver<T> : IObserver<T>, IDisposable
    {
        private readonly IDocumentProcessor<T> _documentProcessor;
        private readonly Action _onPipelineCompleted;
        private bool _sourceComplete;

        private readonly WorkPool _workPool;
        private readonly TaskScheduler _callbackScheduler;

        internal ConcurrentPipelineObserver(
            IDocumentProcessor<T> documentProcessor,
            Action onPipelineCompleted,
            int maxConcurrency,
            TaskScheduler callbackScheduler)
        {
            _documentProcessor = documentProcessor;
            _onPipelineCompleted = onPipelineCompleted;
            _callbackScheduler = callbackScheduler;
            _workPool = new WorkPool(maxConcurrency);
        }

        public void OnCompleted()
        {
            _sourceComplete = true;
        }

        public void OnError(Exception error)
        {
            throw new PipelineException(
                $"A fatal error occurred in the processing pipeline: {error.Message}", error);
        }

        public void OnNext(T doc)
        {
            Task.Run(() => OnNextAsync(doc)).Wait();
        }

        private async Task OnNextAsync(T doc)
        {
            await _workPool
                .Post(() =>
                {
                    return _documentProcessor.Run(doc)
                        .ContinueWith(_ =>
                        {
                            if (_sourceComplete && !_workPool.IsRunning)
                            {
                                _onPipelineCompleted();
                            }
                        }, _callbackScheduler);
                }).ConfigureAwait(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _workPool?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}