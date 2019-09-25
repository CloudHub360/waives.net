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
            var sleepDurationProvider = new ExponentialBackoffSleepProvider();

            _policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(8,
                    sleepDurationProvider.GetSleepDuration,
                    LogRetryAttempt);

            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
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
                RetryLogger.Warn("Request '{RequestMethod} {RequestUri}' failed with " +
                    "{StatusCode}. Retry {RetryAttempt} will happen in {RetryDelay} ms",
                    request.Method, request.RequestUri, response.StatusCode, retryCount,
                    timeSpan.TotalMilliseconds);
            }

            var exception = result.Exception;
            if (exception != null)
            {
                RetryLogger.WarnException("Request failed: {Message}. Retry {RetryAttempt} will happen in " +
                    "{RetryDelay} ms", exception, exception.Message, retryCount, timeSpan.TotalMilliseconds);
            }

            return Task.CompletedTask;
        }
    }
}