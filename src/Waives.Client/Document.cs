using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Client.Responses;

namespace Waives.Client
{
    public class Document
    {
        private readonly HttpClient _httpClient;
        private readonly IDictionary<string, HalUri> _behaviours;

        internal Document(HttpClient httpClient, IDictionary<string, HalUri> behaviours)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _behaviours = behaviours ?? throw new ArgumentNullException(nameof(behaviours));
        }

        public async Task Read(string resultsFilename, string contentType = null)
        {
            contentType = contentType ?? ContentTypes.WaivesReadResults;

            var readResponse = await _httpClient.PutAsync(
                _behaviours["document:read"].CreateUri(),
                new StringContent(string.Empty)).ConfigureAwait(false);

            if (!readResponse.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed initiating read on document.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _behaviours["document:read"].CreateUri());
            request.Headers.Add("Accept", contentType);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

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
            var response = await _waivesClient.HttpClient.DeleteAsync(_behaviours["self"].CreateUri()).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException($"Failed to delete the document.");
            }
        }
    }
}