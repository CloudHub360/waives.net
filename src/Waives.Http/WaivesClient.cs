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
            var response = await HttpClient.PostAsync("/documents", requestBody).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            return new Document(this, behaviours);
        }

        public async Task<Document> CreateDocument(string path)
        {
            var response = await HttpClient.PostAsync("/documents", new StreamContent(File.OpenRead(path))).ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            return new Document(this, behaviours);
        }

        public async Task<Classifier> CreateClassifier(string name, string samplesPath = null)
        {
            var response = await HttpClient.PostAsync($"/classifiers/{name}", null).ConfigureAwait(false);
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
            var response = await HttpClient.GetAsync($"/classifiers/{name}").ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            return new Classifier(this, name, behaviours);
        }

        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            var response = await HttpClient.GetAsync("/documents").ConfigureAwait(false);
            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<DocumentCollection>().ConfigureAwait(false);
            return responseContent.Documents.Select(d => new Document(this, d.Links));
        }

        public async Task Login(string clientId, string clientSecret)
        {
            var response = await HttpClient.PostAsync("/oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
            })).ConfigureAwait(false);

            await EnsureSuccessStatus(response).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<AccessToken>().ConfigureAwait(false);
            var accessToken = responseContent.Token;

            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
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