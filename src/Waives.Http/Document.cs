using System;
using System.Collections.Generic;
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
        private readonly string _id;

        internal Document(WaivesClient httpClient, IDictionary<string, HalUri> behaviours, string id)
        {
            _waivesClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _behaviours = behaviours ?? throw new ArgumentNullException(nameof(behaviours));
            _id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public async Task Read(string resultsFilename, string contentType = null)
        {
            contentType = contentType ?? ContentTypes.WaivesReadResults;

            var readResponse = await _waivesClient.HttpClient.PutAsync(
                _behaviours["document:read"].CreateUri(),
                new StringContent(string.Empty)).ConfigureAwait(false);

            if (!readResponse.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed initiating read on document.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _behaviours["document:read"].CreateUri());
            request.Headers.Add("Accept", contentType);
            var response = await _waivesClient.HttpClient.SendAsync(request).ConfigureAwait(false);

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
            _waivesClient.Logger.Log(LogLevel.Info, $"Deleting document {_id}");
            var response = await _waivesClient.HttpClient.DeleteAsync(_behaviours["self"].CreateUri()).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException($"Failed to delete the document.");
            }
        }

        public async Task<ClassificationResult> Classify(string classifierName)
        {
            var requestUri = _behaviours["document:classify"].CreateUri(new
            {
                classifier_name = classifierName
            });

            var response = await _waivesClient.HttpClient.PostAsync(requestUri, null).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException($"Failed to classify the document with classifier '{classifierName}'");
            }

            var responseBody = await response.Content.ReadAsAsync<ClassificationResponse>().ConfigureAwait(false);
            return responseBody.ClassificationResults;
        }
    }
}