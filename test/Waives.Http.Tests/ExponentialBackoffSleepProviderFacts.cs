using Xunit;

namespace Waives.Http.Tests
{
    public class ExponentialBackoffSleepProviderFacts
    {
        [Theory]
        [InlineData(1, 1000, 2000)]
        [InlineData(2, 2000, 3000)]
        [InlineData(3, 4000, 5000)]
        [InlineData(4, 8000, 9000)]
        [InlineData(5, 16000, 17000)]
        [InlineData(6, 32000, 33000)]
        [InlineData(7, 64000, 65000)]
        [InlineData(8, 128000, 129000)]
        public void Test(int retry, int expectedMinDuration, int expectedMaxDuration)
        {
            var sut = new ExponentialBackoffSleepProvider();
            var timespan = sut.GetSleepDuration(retry);

            Assert.InRange(timespan.TotalMilliseconds, expectedMinDuration, expectedMaxDuration);
        }
    }
}