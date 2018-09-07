using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.RequestHandling;
using Waives.Http.Responses;

namespace Waives.Http
{
    /// <summary>
    /// A client for the Waives' Documents API. Instances can be obtained
    /// via document operations on <see cref="WaivesClient"/>.
    /// </summary>
    public class Document
    {
        private readonly IHttpRequestSender _requestSender;
        private readonly IDictionary<string, HalUri> _behaviours;

        /// <summary>
        /// Gets the string identifier of this document in the Waives platform.
        /// </summary>
        public string Id { get; }

        internal Document(IHttpRequestSender requestSender, string id, IDictionary<string, HalUri> behaviours)
        {
            _requestSender = requestSender ?? throw new ArgumentNullException(nameof(requestSender));
            _behaviours = behaviours ?? throw new ArgumentNullException(nameof(behaviours));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// Deletes this document in the Waives platform.
        /// </summary>
        public async Task Delete()
        {
            var selfUrl = _behaviours["self"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Delete,
                selfUrl.CreateUri());

            await _requestSender.Send(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs classification on this document using the given classifer
        /// name. The named classifier must already exist in the Waives platform.
        /// </summary>
        /// <param name="classifierName">The name of the classifier to use.</param>
        /// <returns>The results of the classification operation.</returns>
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

        /// <summary>
        /// Performs extraction on this document using the given extractor
        /// name. The named extractor must already exist in the Waives platform.
        /// </summary>
        /// <param name="extractorName">The name of the extractor to use.</param>
        /// <returns>The results of the extraction operation.</returns>
        public async Task<ExtractionResults> Extract(string extractorName)
        {
            var extractUrl = _behaviours["document:extract"];
            var request = new HttpRequestMessageTemplate(HttpMethod.Post,
                extractUrl.CreateUri(new
                {
                    extractor_name = extractorName
                }));

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsAsync<ExtractionResults>().ConfigureAwait(false);
            return responseBody;
        }
    }
}