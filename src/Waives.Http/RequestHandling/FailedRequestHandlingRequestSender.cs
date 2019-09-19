using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Responses;

namespace Waives.Http.RequestHandling
{
    internal class FailedRequestHandlingRequestSender : IHttpRequestSender
    {
        private readonly IHttpRequestSender _wrappedRequestSender;

        public FailedRequestHandlingRequestSender(IHttpRequestSender underlyingRequestSender)
        {
            _wrappedRequestSender = underlyingRequestSender;
        }

        public int Timeout
        {
            get => _wrappedRequestSender.Timeout;
            set => _wrappedRequestSender.Timeout = value;
        }

        public void Authenticate(AccessToken accessToken)
        {
            _wrappedRequestSender.Authenticate(accessToken);
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request)
        {
            var response = await _wrappedRequestSender.Send(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var responseContentType = response.Content.Headers.ContentType.MediaType;
            if (responseContentType == "application/json")
            {
                var error = await response.Content.ReadAsAsync<ApiError>().ConfigureAwait(false);
                throw new WaivesApiException(error.Message);
            }

            throw new WaivesApiException($"Unknown Waives error occured: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
    }
}