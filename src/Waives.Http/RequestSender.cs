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

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate template)
        {
            var request = new HttpRequestMessage(template.Method, template.RequestUri)
            {
                Content = template.Content
            };

            foreach (var header in template.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return await _httpClient.SendAsync(request).ConfigureAwait(false);
        }
    }
}