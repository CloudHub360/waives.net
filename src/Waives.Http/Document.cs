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

            var requestUri = _behaviours["document:read"].CreateUri();

            _waivesClient.Logger.Log(LogLevel.Trace, $"Sending PUT request to {requestUri}");

            var readResponse = await _waivesClient.HttpClient.PutAsync(
                requestUri,
                new StringContent(string.Empty)).ConfigureAwait(false);

            _waivesClient.Logger.Log(LogLevel.Trace, $"Received response from PUT {requestUri} ({readResponse.StatusCode})");
            if (!readResponse.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed initiating read on document.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", contentType);

            _waivesClient.Logger.Log(LogLevel.Trace, $"Sending GET request to {requestUri}");

            var response = await _waivesClient.HttpClient.SendAsync(request).ConfigureAwait(false);

            _waivesClient.Logger.Log(LogLevel.Trace, $"Received response from GET {requestUri} ({readResponse.StatusCode})");
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
            var requestUri = _behaviours["self"].CreateUri();
            _waivesClient.Logger.Log(LogLevel.Trace, $"Sending DELETE request to {requestUri}");

            var response = await _waivesClient.HttpClient.DeleteAsync(requestUri).ConfigureAwait(false);

            _waivesClient.Logger.Log(LogLevel.Trace, $"Received response from DELETE {requestUri} ({response.StatusCode})");
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed to delete the document.");
            }
        }

        public async Task<ClassificationResult> Classify(string classifierName)
        {
            var requestUri = _behaviours["document:classify"].CreateUri(new
            {
                classifier_name = classifierName
            });

            _waivesClient.Logger.Log(LogLevel.Trace, $"Sending POST request to {requestUri}");

            var response = await _waivesClient.HttpClient.PostAsync(requestUri, null).ConfigureAwait(false);

            _waivesClient.Logger.Log(LogLevel.Trace, $"Received response from POST {requestUri} ({response.StatusCode})");
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException($"Failed to classify the document with classifier '{classifierName}'");
            }

            var responseBody = await response.Content.ReadAsAsync<ClassificationResponse>().ConfigureAwait(false);
            return responseBody.ClassificationResults;
        }
    }
}