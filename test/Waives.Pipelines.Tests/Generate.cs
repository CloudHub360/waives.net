using System;

namespace Waives.Pipelines.Tests
{
    internal static class Generate
    {
        internal static byte[] Bytes()
        {
            var random = new Random();
            var length = random.Next(1, 1000);
            var result = new byte[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = BitConverter.GetBytes(random.Next(0, 255))[0];
            }
            return result;
        }

        public static string String(string prefix = "")
        {
            return $"{prefix}-{Guid.NewGuid()}";
        }
    }
}
