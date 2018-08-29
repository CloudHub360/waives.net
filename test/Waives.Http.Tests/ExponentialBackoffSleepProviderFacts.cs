using Xunit;

namespace Waives.Http.Tests
{
    public class ExponentialBackoffSleepProviderFacts
    {
        [Theory]
        [InlineData(1, 1000, 2000)] //1000 + 1*1000
        [InlineData(2, 2000, 4000)] //2000 + 2*1000
        [InlineData(3, 4000, 7000)] //4000 + 3*1000
        [InlineData(4, 8000, 12000)] //8000 + 4*1000
        [InlineData(5, 16000, 21000)] //16000 + 5*1000
        [InlineData(6, 32000, 38000)] //32000 + 6*1000
        [InlineData(7, 64000, 71000)] //64000 + 7*1000
        [InlineData(8, 128000, 136000)] //128000 + 8*1000
        public void Test(int retry, int expectedMinDuration, int expectedMaxDuration)
        {
            var sut = new ExponentialBackoffSleepProvider();
            var timespan = sut.GetSleepDuration(retry);

            Assert.InRange(timespan.TotalMilliseconds, expectedMinDuration, expectedMaxDuration);
        }
    }
}