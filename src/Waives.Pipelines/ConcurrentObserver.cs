using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class ConcurrentObserver<T> : IObserver<T>, IDisposable
    {
        private readonly Func<T, Task> _process;
        private readonly Action _onPipelineCompleted;
        private readonly Action<T, Exception> _onError;
        private readonly int _maxConcurrency;
        private bool _sourceComplete;
        private readonly SemaphoreSlim _semaphore;

        private readonly TaskScheduler _mainTaskScheduler = TaskScheduler.Current;

        internal ConcurrentObserver(
            Func<T, Task> process,
            Action onPipelineCompleted, Action<T, Exception> onError, int maxConcurrency)
        {
            _process = process;
            _onPipelineCompleted = onPipelineCompleted;
            _onError = onError;
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

        public void OnNext(T item)
        {
            Task.Run(() => OnNextAsync(item)).Wait();
        }

        private async Task OnNextAsync(T item)
        {
            await _semaphore.WaitAsync();

            Task.Run(() => _process(item)
                .ContinueWith(
                    t =>
                    {
                        _onError(item, t.Exception);
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

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
}