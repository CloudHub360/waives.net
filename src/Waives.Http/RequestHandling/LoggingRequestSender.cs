using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Logging;
using Waives.Http.Responses;

namespace Waives.Http.RequestHandling
{
    internal class LoggingRequestSender : IHttpRequestSender
    {
        private readonly ILogger _logger;
        private readonly IHttpRequestSender _wrappedRequestSender;

        public LoggingRequestSender(ILogger logger, IHttpRequestSender wrappedRequestSender)
        {
            _logger = logger ?? new NoopLogger();
            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
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
                if (e.InnerException != null)
                {
                    _logger.Log(LogLevel.Error, $"{e.Message} Inner exception: {e.InnerException.Message}");
                }
                else
                {
                    _logger.Log(LogLevel.Error, e.Message);
                }
                throw;
            }
        }
    }
}