using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Waives.Http
{
    internal class ReliableRequestSender : IHttpRequestSender
    {
        private readonly IHttpRequestSender _wrappedRequestSender;
        private readonly RetryPolicy<HttpResponseMessage> _policy;

        public ReliableRequestSender(Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context> retryAction, IHttpRequestSender wrappedRequestSender)
        {
            var sleepDurationProvider = new ExponentialBackoffSleepProvider();

            _policy = Policy
                .Handle<WaivesApiException>()
                .OrTransientHttpStatusCode()
                .WaitAndRetryAsync(8,
                    sleepDurationProvider.GetSleepDuration,
                    retryAction);

            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request)
        {
            return await _policy
                .ExecuteAsync(() =>
                    _wrappedRequestSender.Send(request))
                .ConfigureAwait(false);
        }
    }
}