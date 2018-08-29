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

        public async Task<HttpResponseMessage> Send(HttpRequestMessage request)
        {
            return await _policy
                .ExecuteAsync(() =>
                    SendClonedRequest(request))
                .ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendClonedRequest(HttpRequestMessage request)
        {
            // HttpClient does not allow an HttpRequestMessage to be sent more than once,
            // which would cause an issue if we retry, so clone the request first and send the
            // clone to the wrapped sender.
            var clonedRequest = await request.CloneAsync().ConfigureAwait(false);
            return await _wrappedRequestSender.Send(clonedRequest).ConfigureAwait(false);
        }
    }
}