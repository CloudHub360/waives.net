using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Waives.Pipelines.Tests
{
    public class WorkPoolFacts : IDisposable
    {
        private WorkPool _sut;

        [Fact]
        public async Task Post_runs_supplied_func()
        {
            _sut = new WorkPool(3);
            var hasRun = false;

            _sut.Post(() =>
            {
                hasRun = true;
                return Task.CompletedTask;
            });
            await _sut.WaitAsync();

            Assert.True(hasRun);
        }

        [Theory]
        [InlineData(1,3)]
        [InlineData(3,3)]
        [InlineData(1000,1000)]
        [InlineData(1000,1001)]
        public async Task Post_runs_to_concurrency_limit(int concurrencyLimit, int postCount)
        {
            _sut = new WorkPool(concurrencyLimit);
            var currentConcurrency = 0;
            var maxObservedConcurrency = 0;

            async Task SimulatedWork()
            {
                Interlocked.Increment(ref currentConcurrency);
                await Task.Delay(10); // simulate work

                lock (_sut)
                {
                    maxObservedConcurrency = Math.Max(maxObservedConcurrency, currentConcurrency);
                }

                Interlocked.Decrement(ref currentConcurrency);
            }

            for (var i = 0; i < postCount; i++)
            {
                _sut.Post(SimulatedWork);
            }
            await _sut.WaitAsync();

            Assert.Equal(concurrencyLimit, maxObservedConcurrency);
        }

        public void Dispose()
        {
            _sut?.Dispose();
        }
    }
}