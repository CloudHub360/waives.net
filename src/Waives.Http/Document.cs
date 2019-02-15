using System;
using System.Collections.Generic;
using System.IO;
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
        /// Reads text from the document. Subsequent calls to Classify and Extract
        /// will use the results from this operation.
        /// </summary>
        /// <remarks>Not all types of files support this operation.
        /// See the documentation for a list of supported file types. If you are doing
        /// multiple classifications or extractions with different configurations
        /// it is most efficient to call this method first, so the document is only
        /// read once.</remarks>
        public async Task Read()
        {
            var readUrl = _behaviours["document:read"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Put,
                readUrl.CreateUri());

            await _requestSender.Send(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the results of a document read and writes them in the requested format
        /// to the specified path.
        /// </summary>
        /// <param name="path">The path of the file to write the results to</param>
        /// <param name="format">The format of the results required</param>
        /// <remarks>The Read method must be called before this method, otherwise a
        /// WaivesApiException will be thrown.</remarks>
        public async Task GetReadResults(string path, ReadResultsFormat format)
        {
            using (var fileStream = File.OpenWrite(path))
            {
                await GetReadResults(fileStream, format).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the results of a document read and writes them in the requested format
        /// to the specified path.
        /// </summary>
        /// <param name="resultsStream">The stream to write the results to</param>
        /// <param name="format">The format of the results required</param>
        /// <remarks>The Read method must be called before this method, otherwise a
        /// WaivesApiException will be thrown.</remarks>
        public async Task GetReadResults(Stream resultsStream, ReadResultsFormat format)
        {
            var readUrl = _behaviours["document:read"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Get,
                readUrl.CreateUri(),
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Accept", format.ToMimeType())
                });

            var response = await _requestSender.Send(request).ConfigureAwait(false);
            var responseBody = await response
                .Content
                .ReadAsStreamAsync()
                .ConfigureAwait(false);

            await responseBody
                .CopyToAsync(resultsStream)
                .ConfigureAwait(false);
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