using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace Waives.Http
{
    internal class RequestSender : IHttpRequestSender
    {
        private readonly HttpClient _httpClient;

        internal RequestSender(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // This is equivalent to the value used by NuGet
            var productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Waives.NET", productVersion));
        }

        public void Authenticate(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}");
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate template)
        {
            var request = HttpRequestMessageBuilder.BuildRequest(template);

            return await _httpClient.SendAsync(request).ConfigureAwait(false);
        }
    }
}