using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.RequestHandling;
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
            var readRequest = new HttpRequestMessageTemplate(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(string.Empty)
            };

            await _requestSender.Send(readRequest).ConfigureAwait(false);
        }

        private async Task<HttpContent> GetReadResults(Uri requestUri, string contentType)
        {
            var request = new HttpRequestMessageTemplate(HttpMethod.Get, requestUri);
            request.Headers.Add(new KeyValuePair<string, string>("Accept", contentType));

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            return response.Content;
        }

        public async Task Delete()
        {
            var selfUrl = _behaviours["self"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Delete,
                selfUrl.CreateUri());

            await _requestSender.Send(request).ConfigureAwait(false);
        }

        public async Task<ClassificationResult> Classify(string classifierName)
        {
            var classifyUrl = _behaviours["document:classify"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Post,
                classifyUrl.CreateUri(new
                {
                    classifier_name = classifierName
                }));

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsAsync<ClassificationResponse>().ConfigureAwait(false);
            return responseBody.ClassificationResults;
        }

        public async Task<ExtractionResponse> Extract(string extractorName)
        {
            var extractUrl = _behaviours["document:extract"];
            var request = new HttpRequestMessageTemplate(HttpMethod.Post,
                extractUrl.CreateUri(new
                {
                    classifier_name = extractorName
                }));

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsAsync<ExtractionResponse>().ConfigureAwait(false);
            return responseBody;
        }
    }
}