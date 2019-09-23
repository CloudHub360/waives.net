using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Waives.Http.Logging;

namespace Waives.Http.RequestHandling
{
    internal class ReliableRequestSender : IHttpRequestSender
    {
        private static readonly ILog RetryLogger = LogProvider.GetCurrentClassLogger();
        private readonly IHttpRequestSender _wrappedRequestSender;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;

        public ReliableRequestSender(IHttpRequestSender wrappedRequestSender)
        {
            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));

            var sleepDurationProvider = new ExponentialBackoffSleepProvider();

            _policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(8,
                    sleepDurationProvider.GetSleepDuration,
                    LogRetryAttempt);
        }

        public int Timeout
        {
            get => _wrappedRequestSender.Timeout;
            set => _wrappedRequestSender.Timeout = value;
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request)
        {
            return await _policy
                .ExecuteAsync(() =>
                    _wrappedRequestSender.Send(request))
                .ConfigureAwait(false);
        }

        private Task LogRetryAttempt(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount, Context context)
        {
            var response = result.Result;
            if (response != null)
            {
                var request = response.RequestMessage;
                RetryLogger.Warn(
                    $"Request '{request.Method} {request.RequestUri}' failed: " +
                    $"{(int)response.StatusCode} {response.ReasonPhrase}. Retry {retryCount} " +
                    $"will happen in {timeSpan.TotalMilliseconds} ms");
            }

            var exception = result.Exception;
            if (exception != null)
            {
                RetryLogger.Warn(
                    $"Request failed: {exception.Message}. Retry {retryCount} " +
                    $"will happen in {timeSpan.TotalMilliseconds} ms");
            }

            return Task.CompletedTask;
        }
    }
}