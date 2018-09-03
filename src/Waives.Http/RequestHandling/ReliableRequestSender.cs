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
        private readonly ILogger _retryLogger;
        private readonly IHttpRequestSender _wrappedRequestSender;
        private readonly RetryPolicy<HttpResponseMessage> _policy;

        public ReliableRequestSender(ILogger retryLogger, IHttpRequestSender wrappedRequestSender)
        {
            var sleepDurationProvider = new ExponentialBackoffSleepProvider();

            _policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(8,
                    sleepDurationProvider.GetSleepDuration,
                    LogRetryAttempt);

            _retryLogger = retryLogger ?? throw new ArgumentNullException(nameof(retryLogger));
            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
        }

        public int Timeout
        {
            get => _wrappedRequestSender.Timeout;
            set => _wrappedRequestSender.Timeout = value;
        }

        public void Authenticate(string accessToken)
        {
            _wrappedRequestSender.Authenticate(accessToken);
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
                _retryLogger.Log(
                    LogLevel.Warn,
                    $"Request '{request.Method} {request.RequestUri}' failed: " +
                    $"{(int)response.StatusCode} {response.ReasonPhrase}. Retry {retryCount} " +
                    $"will happen in {timeSpan.TotalMilliseconds} ms");
            }

            var exception = result.Exception;
            if (exception != null)
            {
                _retryLogger.Log(
                    LogLevel.Warn,
                    $"Request failed: {exception.Message}. Retry {retryCount} " +
                    $"will happen in {timeSpan.TotalMilliseconds} ms");
            }

            return Task.CompletedTask;
        }
    }
}