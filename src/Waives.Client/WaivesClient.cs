using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waives.Client.Responses;

[assembly: InternalsVisibleTo("Waives.Client.Tests")]
namespace Waives.Client
{
    public class WaivesClient
    {
        private readonly HttpClient _httpClient;

        public WaivesClient() : this(new HttpClient { BaseAddress = new Uri("https://api.waives.io") }) { }

        internal WaivesClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<Document> CreateDocument(string path)
        {
            var response = await _httpClient.PostAsync("/documents", new StreamContent(File.OpenRead(path))).ConfigureAwait(false);
            EnsureSuccessStatus(response);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            return new Document(_httpClient, behaviours);
        }

        public async Task<Classifier> CreateClassifier(string name, string samplesPath)
        {
            var response = await _httpClient.PostAsync($"/classifiers/{name}", null).ConfigureAwait(false);
            EnsureSuccessStatus(response);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            var classifier = new Classifier(_httpClient, behaviours);

            await classifier.AddSamplesFromZip(samplesPath);

            return classifier;
        }

        public async Task Login(string clientId, string clientSecret)
        {
            var response = await _httpClient.PostAsync("/oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
            })).ConfigureAwait(false);

            EnsureSuccessStatus(response);

            var responseContent = await response.Content.ReadAsAsync<AccessToken>().ConfigureAwait(false);
            var accessToken = responseContent.Token;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        }

        private static void EnsureSuccessStatus(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            throw new WaivesApiException($"Request failed with reponse {response.StatusCode}.");
        }
    }
}