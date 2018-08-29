using System;

namespace Waives.Http
{
    public class ExponentialBackoffSleepProvider
    {
        private readonly Random _jitterer = new Random();

        // With 8 retries, we get retries at the following base times:
        // 0s, 1s, 3s, 7s, 15s, 31s, 63s and 127s
        // But we will also have additional jitter time of between
        // 0 and 36s (1+2+..+8s)
        public TimeSpan GetSleepDuration(int retry)
        {
            var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retry - 1));

            // Add some jitter so we spread out retried requests if we had
            // a system glitch that affected multiple requests
            var jitterDelay = TimeSpan.FromMilliseconds(
                _jitterer.Next(0, 1000 * retry));

            return baseDelay + jitterDelay;
        }
    }
}