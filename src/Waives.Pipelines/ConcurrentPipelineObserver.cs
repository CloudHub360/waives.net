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

        private readonly TaskScheduler _mainTaskScheduler = TaskScheduler.Current;

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
            Console.WriteLine("OnComplete");
            _sourceComplete = true;
        }

        public void OnError(Exception error)
        {
            throw new PipelineException(error);
        }

        public void OnNext(WaivesDocument doc)
        {
            Console.WriteLine("OnNext");

            Task.Run(() => OnNextAsync(doc)).Wait();
        }

        private async Task OnNextAsync(WaivesDocument doc)
        {
            await _semaphore.WaitAsync();

            Console.WriteLine("In progress: " + (_maxConcurrency - _semaphore.CurrentCount));

            Task.Run(() => PerformDocActions(doc)
                .ContinueWith(
                    t =>
                    {
                        _onDocumentError(new DocumentError(doc.Source, t.Exception));
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    _mainTaskScheduler)
                .ContinueWith(_ =>
                {
                    _semaphore.Release();
                })
                .ContinueWith(_ =>
                {
                    if (_sourceComplete && _semaphore.CurrentCount == _maxConcurrency)
                    {
                        _onPipelineCompleted();
                    }
                }, _mainTaskScheduler));
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