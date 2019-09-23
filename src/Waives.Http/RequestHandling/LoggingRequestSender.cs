using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Logging;

namespace Waives.Http.RequestHandling
{
    internal class LoggingRequestSender : IHttpRequestSender
    {
        private static readonly ILog Logger = LogProvider.For<RequestSender>();
        private readonly IHttpRequestSender _wrappedRequestSender;

        public LoggingRequestSender(IHttpRequestSender wrappedRequestSender)
        {
            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
        }

        public int Timeout
        {
            get => _wrappedRequestSender.Timeout;
            set => _wrappedRequestSender.Timeout = value;
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessageTemplate request)
        {
            var stopWatch = new Stopwatch();
            Logger.TraceFormat("Sending {RequestMethod} request to {RequestUri}", request.Method, request.RequestUri);

            try
            {
                stopWatch.Start();
                var response = await _wrappedRequestSender.Send(request).ConfigureAwait(false);
                stopWatch.Stop();

                Logger.TraceFormat(
                    "Received response from {RequestMethod} {RequestUri} ({StatusCode}) ({ElapsedMilliseconds} ms)",
                    request.Method, request.RequestUri, (int)response.StatusCode, stopWatch.ElapsedMilliseconds);

                return response;
            }
            catch (WaivesApiException e)
            {
                if (e.InnerException != null)
                {
                    Logger.ErrorException($"{e.Message} Inner exception: {e.InnerException}", e);
                }
                else
                {
                    Logger.ErrorException(e.Message, e);
                }
                throw;
            }
        }
    }
}