using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Waives.Http.RequestHandling
{
    class TokenFetchingRequestSender : IHttpRequestSender, IDisposable
    {
        private readonly AccessTokenService _accessTokenService;
        private readonly IHttpRequestSender _requestSender;

        internal TokenFetchingRequestSender(AccessTokenService accessTokenService, IHttpRequestSender underlyingRequestSender)
        {
            _accessTokenService = accessTokenService;
            _requestSender = underlyingRequestSender;
        }

        public int Timeout
        {
            get => _requestSender.Timeout;
            set => _requestSender.Timeout = value;
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request)
        {
            var token = await _accessTokenService.FetchAccessTokenAsync().ConfigureAwait(false);
            request.Headers["Authorization"] = $"Bearer {token}";

            return await _requestSender.Send(request).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _accessTokenService.Dispose();
        }
    }
}