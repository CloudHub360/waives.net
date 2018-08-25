using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Logging;
using Waives.Http.Responses;

namespace Waives.Http
{
    public class Document
    {
        private readonly WaivesClient _waivesClient;
        private readonly IDictionary<string, HalUri> _behaviours;
        internal readonly string Id;

        internal Document(WaivesClient httpClient, IDictionary<string, HalUri> behaviours, string id)
        {
            _waivesClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _behaviours = behaviours ?? throw new ArgumentNullException(nameof(behaviours));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public async Task Read(string resultsFilename, string contentType = null)
        {
            contentType = contentType ?? ContentTypes.WaivesReadResults;

            var requestUri = _behaviours["document:read"].CreateUri();
            var readRequest = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(string.Empty)
            };
            var readResponse = await _waivesClient.SendRequest(readRequest).ConfigureAwait(false);
            if (!readResponse.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed initiating read on document.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", contentType);

            var response = await _waivesClient.SendRequest(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed retrieving document read results.");
            }

            var resultsStream = response.Content;

            using (var fileStream = File.OpenWrite(resultsFilename))
            {
                await resultsStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }
        }

        public async Task Delete()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete,
                _behaviours["self"].CreateUri());

            var response = await _waivesClient.SendRequest(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed to delete the document.");
            }
        }

        public async Task<ClassificationResult> Classify(string classifierName)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                _behaviours["document:classify"].CreateUri(new
                {
                    classifier_name = classifierName
                }));

            var response = await _waivesClient.SendRequest(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException($"Failed to classify the document with classifier '{classifierName}'");
            }

            var responseBody = await response.Content.ReadAsAsync<ClassificationResponse>().ConfigureAwait(false);
            return responseBody.ClassificationResults;
        }
    }
}