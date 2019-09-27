using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class WorkPool : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<Task, bool> _tasks = new ConcurrentDictionary<Task, bool>();

        public WorkPool(int maxConcurrency)
        {
            _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        /// <summary>
        /// Post accepts an async function to run concurrently, up to the maximum level
        /// specified in the constructor. If the WorkPool is already running at the maximum
        /// level of concurrency, Post blocks until one of the in-flight Tasks has completed.
        /// </summary>
        /// <param name="action"></param>
        public void Post(Func<Task> action)
        {
            _semaphore.Wait();

            var task = action();
            _tasks.TryAdd(task, true);

            task.ContinueWith(t =>
            {
                _tasks.TryRemove(t, out _);
                _semaphore.Release();
            }, TaskContinuationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Waits for all currently executing tasks.
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync()
        {
            await _semaphore.WaitAsync();
            await Task.WhenAll(_tasks.Keys.ToArray());
            _semaphore.Release();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}