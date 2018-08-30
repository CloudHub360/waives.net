using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Responses;

namespace Waives.Http
{
    public class Document
    {
        private readonly IHttpRequestSender _requestSender;
        private readonly IDictionary<string, HalUri> _behaviours;
        public string Id { get; }

        internal Document(IHttpRequestSender requestSender, string id, IDictionary<string, HalUri> behaviours)
        {
            _requestSender = requestSender ?? throw new ArgumentNullException(nameof(requestSender));
            _behaviours = behaviours ?? throw new ArgumentNullException(nameof(behaviours));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public async Task Read(string resultsFilename, string contentType = null)
        {
            contentType = contentType ?? ContentTypes.WaivesReadResults;
            var requestUri = _behaviours["document:read"].CreateUri();

            await DoRead(requestUri).ConfigureAwait(false);
            var httpContent = await GetReadResults(requestUri, contentType).ConfigureAwait(false);

            using (var fileStream = File.OpenWrite(resultsFilename))
            {
                await httpContent.CopyToAsync(fileStream).ConfigureAwait(false);
            }
        }

        private async Task DoRead(Uri requestUri)
        {
            var readRequest = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(string.Empty)
            };

            var readResponse = await _requestSender.Send(readRequest).ConfigureAwait(false);
            if (!readResponse.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed initiating read on document.");
            }
        }

        private async Task<HttpContent> GetReadResults(Uri requestUri, string contentType)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", contentType);

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed retrieving document read results.");
            }

            return response.Content;
        }

        public async Task Delete()
        {
            var selfUrl = _behaviours["self"];

            var request = new HttpRequestMessage(HttpMethod.Delete,
                selfUrl.CreateUri());

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException("Failed to delete the document.");
            }
        }

        public async Task<ClassificationResult> Classify(string classifierName)
        {
            var classifyUrl = _behaviours["document:classify"];

            var request = new HttpRequestMessage(HttpMethod.Post,
                classifyUrl.CreateUri(new
                {
                    classifier_name = classifierName
                }));

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException($"Failed to classify the document with classifier '{classifierName}'");
            }

            var responseBody = await response.Content.ReadAsAsync<ClassificationResponse>().ConfigureAwait(false);
            return responseBody.ClassificationResults;
        }

        public async Task<IEnumerable<ExtractionResult>> Extract(string extractorName)
        {
            var extractUrl = _behaviours["document:extract"];
            var request = new HttpRequestMessage(HttpMethod.Post,
                extractUrl.CreateUri(new
                {
                    classifier_name = extractorName
                }));

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException($"Failed to extract data from the document using extractor '{extractorName}'");
            }

            var responseBody = await response.Content.ReadAsAsync<ExtractionResponse>().ConfigureAwait(false);
            return responseBody.ExtractionResults;
        }
    }
}