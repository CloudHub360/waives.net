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
[assembly: InternalsVisibleTo("Waives.Pipelines")]
namespace Waives.Http
{
    public class WaivesClient
    {
        internal ILogger Logger { get; }
        internal HttpClient HttpClient { get; }
        private const string DefaultUrl = "https://api.waives.io";

        public WaivesClient() : this(new HttpClient { BaseAddress = new Uri(DefaultUrl)}, Loggers.NoopLogger)
        { }

        public WaivesClient(ILogger logger) : this(new HttpClient { BaseAddress = new Uri(DefaultUrl) }, logger)
        {
        }

        internal WaivesClient(HttpClient httpClient) : this(httpClient, Loggers.NoopLogger)
        { }

        private WaivesClient(HttpClient httpClient, ILogger logger)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Document> CreateDocument(Stream documentSource)
        {
            var requestBody = new StreamContent(documentSource);
            Logger.Log(LogLevel.Trace, "Sending POST request to /documents");
            var response = await HttpClient.PostAsync("/documents", requestBody).ConfigureAwait(false);
            Logger.Log(LogLevel.Trace, $"Received response from POST /documents ({response.StatusCode})");

            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            var document = new Document(this, behaviours);

            Logger.Log(LogLevel.Info, "Created document");
            return document;
        }

        public async Task<Document> CreateDocument(string path)
        {
            Logger.Log(LogLevel.Trace, "Sending POST request to /documents");
            var response = await HttpClient.PostAsync("/documents", new StreamContent(File.OpenRead(path))).ConfigureAwait(false);
            Logger.Log(LogLevel.Trace, $"Received response from POST /documents ({response.StatusCode})");

            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            var document = new Document(this, behaviours);

            Logger.Log(LogLevel.Info, $"Created document from '{path}'");
            return document;
        }

        public async Task<Classifier> CreateClassifier(string name, string samplesPath = null)
        {
            Logger.Log(LogLevel.Trace, $"Sending POST request to /classifiers/{name}");
            var response = await HttpClient.PostAsync($"/classifiers/{name}", null).ConfigureAwait(false);
            Logger.Log(LogLevel.Trace, $"Received response from POST /classifiers/{name} ({response.StatusCode})");

            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            var classifier = new Classifier(this, name, behaviours);

            if (!string.IsNullOrWhiteSpace(samplesPath))
            {
                await classifier.AddSamplesFromZip(samplesPath).ConfigureAwait(false);
            }

            Logger.Log(LogLevel.Info, $"Created classifier '{name}' from samples zip file '{samplesPath}'");
            return classifier;
        }

        public async Task<Classifier> GetClassifier(string name)
        {
            Logger.Log(LogLevel.Trace, $"Sending GET request to /classifiers/{name}");
            var response = await HttpClient.GetAsync($"/classifiers/{name}").ConfigureAwait(false);
            Logger.Log(LogLevel.Trace, $"Received response from GET /classifiers/{name} ({response.StatusCode})");

            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            var classifier = new Classifier(this, name, behaviours);

            Logger.Log(LogLevel.Info, $"Retrieved details of classifier '{name}'");
            return classifier;
        }

        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            Logger.Log(LogLevel.Trace, "Sending GET request to /documents");
            var response = await HttpClient.GetAsync("/documents").ConfigureAwait(false);
            Logger.Log(LogLevel.Trace, $"Received response from GET /documents ({response.StatusCode})");

            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<DocumentCollection>().ConfigureAwait(false);
            var documents =  responseContent.Documents.Select(d => new Document(this, d.Links));

            Logger.Log(LogLevel.Info, "Retrieved details of all current documents");
            return documents;
        }

        public async Task Login(string clientId, string clientSecret)
        {
            Logger.Log(LogLevel.Trace, "Sending POST request to /oauth/token");
            var response = await HttpClient.PostAsync("/oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
            })).ConfigureAwait(false);
            Logger.Log(LogLevel.Trace, $"Received response from POST /oauth/token ({response.StatusCode})");

            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<AccessToken>().ConfigureAwait(false);
            var accessToken = responseContent.Token;

            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            Logger.Log(LogLevel.Info, $"Logged in to Waives at '{HttpClient.BaseAddress}'");
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