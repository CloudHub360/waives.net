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
        internal HttpClient HttpClient { get; }

        public WaivesClient() : this(new HttpClient { BaseAddress = new Uri("https://api.waives.io") }) { }

        internal WaivesClient(HttpClient httpClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<Document> CreateDocument(string path)
        {
            var response = await HttpClient.PostAsync("/documents", new StreamContent(File.OpenRead(path))).ConfigureAwait(false);
            EnsureSuccessStatus(response);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            return new Document(_httpClient, behaviours);
        }

        public async Task<Classifier> CreateClassifier(string name, string samplesPath = null)
        {
            var response = await HttpClient.PostAsync($"/classifiers/{name}", null).ConfigureAwait(false);
            EnsureSuccessStatus(response);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var behaviours = responseContent.Links;

            var classifier = new Classifier(_httpClient, behaviours);

            if (!string.IsNullOrWhiteSpace(samplesPath))
            {
                await classifier.AddSamplesFromZip(samplesPath).ConfigureAwait(false);
            }

            return classifier;
        }

        public async Task Login(string clientId, string clientSecret)
        {
            var response = await HttpClient.PostAsync("/oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
            })).ConfigureAwait(false);

            EnsureSuccessStatus(response);

            var responseContent = await response.Content.ReadAsAsync<AccessToken>().ConfigureAwait(false);
            var accessToken = responseContent.Token;

            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        }

        private static void EnsureSuccessStatus(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            throw new WaivesApiException($"Request failed with response {(int)response.StatusCode} {response.StatusCode}.");
        }
    }
}