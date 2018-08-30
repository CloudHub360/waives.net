using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waives.Http.Logging;
using Waives.Http.Responses;

[assembly: InternalsVisibleTo("Waives.Http.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Waives.Pipelines")]
namespace Waives.Http
{
    public class WaivesClient
    {
        internal const string DefaultUrl = "https://api.waives.io";

        private readonly IHttpRequestSender _requestSender;

        public WaivesClient(Uri apiUrl = null, ILogger logger = null)
            : this(apiUrl, logger, null)
        {
        }

        internal WaivesClient(Uri apiUrl = null, ILogger logger = null, IHttpRequestSender requestSender = null)
        {
            logger = logger ?? new NoopLogger();

            _requestSender = requestSender ??
                 new ReliableRequestSender(
                     (result, timeSpan, retryCount, ctx) =>
                     {
                         logger.Log(LogLevel.Warn, $"Request failed. Retry {retryCount} will happen in {timeSpan.TotalMilliseconds} ms");
                     },
                     new LoggingRequestSender(
                         new ExceptionHandlingRequestSender(
                             new RequestSender(
                                 new HttpClient
                                 {
                                     BaseAddress = apiUrl ?? new Uri(DefaultUrl)
                                 })),
                         logger));
        }

        /// <summary>
        /// Gets or sets a duration on the underlying <see cref="System.Net.Http.HttpClient"/>
        /// to wait until the requests time out. The timeout unit is seconds, and defaults to 120.
        /// </summary>
        /// <seealso cref="System.Net.Http.HttpClient.Timeout"/>
        public int Timeout
        {
            get => _requestSender.Timeout;
            set => _requestSender.Timeout = value;
        }

        public async Task<Document> CreateDocument(Stream documentSource)
        {
            var request =
                new HttpRequestMessageTemplate(HttpMethod.Post, new Uri($"/documents", UriKind.Relative))
                {
                    Content = new StreamContent(documentSource)
                };

            return await CreateDocument(request).ConfigureAwait(false);
        }

        public async Task<Document> CreateDocument(string path)
        {
            var request =
                new HttpRequestMessageTemplate(HttpMethod.Post, new Uri($"/documents", UriKind.Relative))
                {
                    Content = new StreamContent(File.OpenRead(path))
                };

            return await CreateDocument(request).ConfigureAwait(false);
        }

        private async Task<Document> CreateDocument(HttpRequestMessageTemplate request)
        {
            var response = await _requestSender.Send(request).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var id = responseContent.Id;
            var behaviours = responseContent.Links;

            var document = new Document(_requestSender, id, behaviours);

            return document;
        }

        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            var request = new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative));
            var response = await _requestSender.Send(request).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<DocumentCollection>().ConfigureAwait(false);
            return responseContent.Documents.Select(d => new Document(_requestSender, d.Id, d.Links));
        }

        public async Task Login(string clientId, string clientSecret)
        {
            var request = new HttpRequestMessageTemplate(HttpMethod.Post, new Uri("/oauth/token", UriKind.Relative))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"client_id", clientId},
                    {"client_secret", clientSecret}
                })
            };

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<AccessToken>().ConfigureAwait(false);
            var accessToken = responseContent.Token;

            _requestSender.Authenticate(accessToken);
        }

        private static async Task EnsureSuccessStatus(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var responseContentType = response.Content.Headers.ContentType.MediaType;
            if (responseContentType == "application/json")
            {
                var error = await response.Content.ReadAsAsync<Error>().ConfigureAwait(false);
                throw new WaivesApiException(error.Message);
            }

            throw new WaivesApiException($"Unknown Waives error occured: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
    }
}