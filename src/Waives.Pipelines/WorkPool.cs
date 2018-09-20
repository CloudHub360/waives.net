using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    public class WorkPool : IDisposable
    {
        private readonly int _maxConcurrency;
        private readonly SemaphoreSlim _semaphoreSlim;

        public WorkPool(int maxConcurrency)
        {
            _maxConcurrency = maxConcurrency;
            _semaphoreSlim = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        public async Task Post(Func<Task> action)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

            _ = Task.Run(action)
                .ContinueWith(_ => { _semaphoreSlim.Release(); });
        }

        public bool IsRunning => _semaphoreSlim.CurrentCount == _maxConcurrency;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphoreSlim?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}