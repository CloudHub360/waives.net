using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Waives.Http.Logging;

namespace Waives.Http
{
    internal class ReliableRequestSender : IHttpRequestSender
    {
        private readonly ILogger _retryLogger;
        private readonly IHttpRequestSender _wrappedRequestSender;
        private readonly RetryPolicy<HttpResponseMessage> _policy;

        public ReliableRequestSender(ILogger retryLogger, IHttpRequestSender wrappedRequestSender)
        {
            var sleepDurationProvider = new ExponentialBackoffSleepProvider();

            _policy = Policy
                .Handle<WaivesApiException>()
                .OrTransientHttpStatusCode()
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
            _retryLogger.Log(LogLevel.Warn, $"Request failed. Retry {retryCount} will happen in {timeSpan.TotalMilliseconds} ms");
            return Task.CompletedTask;
        }
    }
}