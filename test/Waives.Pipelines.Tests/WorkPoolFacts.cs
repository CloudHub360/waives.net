using System;
using System.Threading.Tasks;
using Xunit;

namespace Waives.Pipelines.Tests
{
    public class WorkPoolFacts
    {
        [Fact]
        public async Task Post_runs_supplied_func()
        {
            var sut = new WorkPool(3);
            var hasRun = false;

            sut.Post(() =>
            {
                hasRun = true;
                return Task.CompletedTask;
            });
            await sut.WaitAsync();

            Assert.True(hasRun);
        }

        [Theory]
        [InlineData(1,3)]
        [InlineData(3,3)]
        [InlineData(100,100)]
        [InlineData(100,300)]
        public async Task Post_runs_to_concurrency_limit(int concurrencyLimit, int postCount)
        {
            var sut = new WorkPool(concurrencyLimit);
            var currentConcurrency = 0;
            var maxObservedConcurrency = 0;

            Func<Task> func = async () =>
            {
                currentConcurrency++;
                await Task.Delay(10); // simulate work
                lock (sut)
                {
                    maxObservedConcurrency = Math.Max(maxObservedConcurrency, currentConcurrency);
                }
                currentConcurrency--;
            };
            for (int i = 0; i < postCount; i++)
            {
                sut.Post(func);
            }
            await sut.WaitAsync();


            Assert.Equal(concurrencyLimit, maxObservedConcurrency);
        }
    }
}