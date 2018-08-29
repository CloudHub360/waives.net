using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Logging;

namespace Waives.Http
{
    internal class LoggingRequestSender : IHttpRequestSender
    {
        private readonly IHttpRequestSender _wrappedRequestSender;
        private readonly ILogger _logger;

        public LoggingRequestSender(IHttpRequestSender wrappedRequestSender, ILogger logger)
        {
            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessage request)
        {
            var stopWatch = new Stopwatch();
            _logger.Log(LogLevel.Trace, $"Sending {request.Method} request to {request.RequestUri}");

            try
            {
                stopWatch.Start();
                var response = await _wrappedRequestSender.Send(request).ConfigureAwait(false);
                stopWatch.Stop();

                _logger.Log(LogLevel.Trace,
                    $"Received response from {request.Method} {request.RequestUri} ({response.StatusCode}) ({stopWatch.ElapsedMilliseconds} ms)");

                return response;
            }
            catch (WaivesApiException e)
            {
                _logger.Log(LogLevel.Error,
                    e.InnerException != null ? $"{e.Message} Inner exception: {e.InnerException.Message}" : e.Message);

                throw;
            }
        }
    }
}