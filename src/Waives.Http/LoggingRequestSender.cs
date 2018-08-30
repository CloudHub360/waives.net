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
        internal ILogger Logger;

        public LoggingRequestSender(IHttpRequestSender wrappedRequestSender, ILogger logger)
        {
            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
            Logger = logger ?? new NoopLogger();
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request)
        {
            var stopWatch = new Stopwatch();
            Logger.Log(LogLevel.Trace, $"Sending {request.Method} request to {request.RequestUri}");

            try
            {
                stopWatch.Start();
                var response = await _wrappedRequestSender.Send(request).ConfigureAwait(false);
                stopWatch.Stop();

                Logger.Log(LogLevel.Trace,
                    $"Received response from {request.Method} {request.RequestUri} ({response.StatusCode}) ({stopWatch.ElapsedMilliseconds} ms)");

                return response;
            }
            catch (WaivesApiException e)
            {
                if (e.InnerException != null)
                {
                    Logger.Log(LogLevel.Error, $"{e.Message} Inner exception: {e.InnerException.Message}");
                }
                else
                {
                    Logger.Log(LogLevel.Error, e.Message);
                }
                throw;
            }
        }
    }
}