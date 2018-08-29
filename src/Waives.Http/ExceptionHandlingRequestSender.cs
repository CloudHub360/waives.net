using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Waives.Http
{
    internal class ExceptionHandlingRequestSender : IHttpRequestSender
    {
        private readonly IHttpRequestSender _wrappedRequestSender;

        public ExceptionHandlingRequestSender(IHttpRequestSender wrappedRequestSender)
        {
            _wrappedRequestSender = wrappedRequestSender ?? throw new ArgumentNullException(nameof(wrappedRequestSender));
        }

        /// <summary>
        /// Send a request to the wrapped IHttpRequestSender and make sure that any exception is transformed
        /// into a WaivesApiException.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Send(HttpRequestMessage request)
        {
            try
            {
                return await _wrappedRequestSender.Send(request).ConfigureAwait(false);
            }
            catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
            {
                // Either TaskCanceledException or OperationCanceledException may be thrown by HttpClient if a response
                // is not received before the HttpClient's TimeOut expires
                throw new WaivesApiException($"{request.Method} request to {request.RequestUri} timed-out.");
            }
            catch (Exception e)
            {
                var message = $"An unexpected error occurred making {request.Method} request to {request.RequestUri}. " +
                              $"The error was: {e.Message}.";

                throw new WaivesApiException(message, e);
            }
        }
    }
}