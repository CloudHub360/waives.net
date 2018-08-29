using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Polly;
using Waives.Http.Logging;
using Waives.Http.Responses;

[assembly: InternalsVisibleTo("Waives.Http.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Waives.Pipelines")]
namespace Waives.Http
{
    public class WaivesClient
    {
        private const string DefaultUrl = "https://api.waives.io";

        internal HttpClient HttpClient { get; }

        private readonly IHttpRequestSender _requestSender;
        private readonly LoggingRequestSender _loggingRequestSender;
        private ILogger _logger;

        public WaivesClient(Uri apiUrl = null, ILogger logger = null)
            : this(new HttpClient { BaseAddress = apiUrl ?? new Uri(DefaultUrl) }, logger ?? new NoopLogger(), null)
        {
        }

        internal WaivesClient(HttpClient httpClient) : this(httpClient, new NoopLogger(), null)
        { }

        internal WaivesClient(HttpClient httpClient, ILogger logger, IHttpRequestSender requestSender)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _loggingRequestSender = new LoggingRequestSender(
                new ExceptionHandlingRequestSender(
                    new RequestSender(httpClient)),
                logger);
            Logger = logger ?? new NoopLogger();

            _requestSender = requestSender ??
                             new ReliableRequestSender(RetryAction,
                                 _loggingRequestSender);
            Timeout = 120;
        }

        /// <summary>
        /// Gets or sets a duration on the underlying <see cref="HttpClient"/> to wait
        /// until the requests time out. The timeout unit is seconds, and defaults to 120.
        /// </summary>
        /// <seealso cref="System.Net.Http.HttpClient.Timeout"/>
        public int Timeout
        {
            get => HttpClient.Timeout.Seconds;
            set => HttpClient.Timeout = TimeSpan.FromSeconds(value);
        }

        internal ILogger Logger
        {
            get => _logger;
            set
            {
                _logger = value;
                _loggingRequestSender.Logger = value;
            }
        }

        public async Task<Document> CreateDocument(Stream documentSource)
        {
            var request =
                new HttpRequestMessage(HttpMethod.Post, new Uri($"/documents", UriKind.Relative))
                {
                    Content = new StreamContent(documentSource)
                };

            return await CreateDocument(request).ConfigureAwait(false);
        }

        public async Task<Document> CreateDocument(string path)
        {
            var request =
                new HttpRequestMessage(HttpMethod.Post, new Uri($"/documents", UriKind.Relative))
                {
                    Content = new StreamContent(File.OpenRead(path))
                };

            return await CreateDocument(request).ConfigureAwait(false);
        }

        private async Task<Document> CreateDocument(HttpRequestMessage request)
        {
            var response = await _requestSender.Send(request).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var id = responseContent.Id;
            var behaviours = responseContent.Links;

            var document = new Document(_requestSender, id, behaviours);

            Logger.Log(LogLevel.Trace, $"Created Waives document {id}");
            return document;
        }

        public async Task<Classifier> CreateClassifier(string name, string samplesPath = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"/classifiers/{name}", UriKind.Relative));
            var response = await _requestSender.Send(request).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            var classifier = new Classifier(this, name, behaviours);

            if (!string.IsNullOrWhiteSpace(samplesPath))
            {
                await classifier.AddSamplesFromZip(samplesPath).ConfigureAwait(false);
            }

            return classifier;
        }

        public async Task<Classifier> GetClassifier(string name)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"/classifiers/{name}", UriKind.Relative));
            var response = await _requestSender.Send(request).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            return new Classifier(this, name, behaviours);
        }

        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/documents", UriKind.Relative));
            var response = await _requestSender.Send(request).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<DocumentCollection>().ConfigureAwait(false);
            return responseContent.Documents.Select(d => new Document(_requestSender, d.Id, d.Links));
        }

        public async Task Login(string clientId, string clientSecret)
        {
            Logger.Log(LogLevel.Debug, $"Authenticating with Waives at {HttpClient.BaseAddress}");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/oauth/token", UriKind.Relative))
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

            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            Logger.Log(LogLevel.Info, "Logged in.");
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

        private void RetryAction(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, int retryCount, Context context)
        {
            Logger.Log(LogLevel.Warn, $"Request failed. Retry {retryCount} will happen in {timeSpan.TotalMilliseconds} ms");
        }
    }
}