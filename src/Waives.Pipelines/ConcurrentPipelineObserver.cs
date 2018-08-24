using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class ConcurrentPipelineObserver : IObserver<WaivesDocument>, IDisposable
    {
        private readonly IEnumerable<Func<WaivesDocument, Task<WaivesDocument>>> _docActions;
        private readonly Action _onPipelineCompleted;
        private readonly Action<DocumentError> _onDocumentError;
        private readonly int _maxConcurrency;
        private bool _sourceComplete;
        private readonly SemaphoreSlim _semaphore;

        internal ConcurrentPipelineObserver(
            IEnumerable<Func<WaivesDocument, Task<WaivesDocument>>> docActions,
            Action onPipelineCompleted, Action<DocumentError> onDocumentError, int maxConcurrency)
        {
            _docActions = docActions;
            _onPipelineCompleted = onPipelineCompleted;
            _onDocumentError = onDocumentError;
            _maxConcurrency = maxConcurrency;
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        public void OnCompleted()
        {
            _sourceComplete = true;
        }

        public void OnError(Exception error)
        {
            throw new PipelineException(error);
        }

        public void OnNext(WaivesDocument doc)
        {
            Task.Run(() => OnNextAsync(doc)).Wait();
        }

        private async Task OnNextAsync(WaivesDocument doc)
        {
            await _semaphore.WaitAsync();

            _ = PerformDocActions(doc)
                .ContinueWith(t =>
                {
                    _onDocumentError(new DocumentError(doc.Source, t.Exception));
                }, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(_ =>
                {
                    _semaphore.Release();
                })
                .ContinueWith(_ =>
                {
                    if (_sourceComplete && _semaphore.CurrentCount == _maxConcurrency)
                    {
                        // TODO run on main thread
                        _onPipelineCompleted();
                    }
                });
        }

        private async Task PerformDocActions(WaivesDocument doc)
        {
            foreach (var docAction in _docActions)
            {
                // mmm, curry.
                doc = await docAction(doc);
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
}