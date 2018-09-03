using System;
using System.Collections.Generic;
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