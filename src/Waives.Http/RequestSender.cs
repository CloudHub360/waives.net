using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Waives.Http.Logging;

namespace Waives.Http
{
    internal class RequestSender : IHttpRequestSender
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        internal RequestSender(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // This is equivalent to the value used by NuGet
            var productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Waives.NET", productVersion));
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessage request)
        {
            var stopWatch = new Stopwatch();
            _logger.Log(LogLevel.Trace, $"Sending {request.Method} request to {request.RequestUri}");

            stopWatch.Start();
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            stopWatch.Stop();

            _logger.Log(LogLevel.Trace, $"Received response from {request.Method} {request.RequestUri} ({response.StatusCode}) ({stopWatch.ElapsedMilliseconds} ms)");

            return response;
        }
    }
}