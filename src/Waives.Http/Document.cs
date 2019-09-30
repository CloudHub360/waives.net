using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Waives.Http.RequestHandling;
using Waives.Http.Requests;
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
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        public async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            var selfUrl = _behaviours["self"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Delete,
                selfUrl.CreateUri());

            await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads text from the document. Subsequent calls to Classify and Extract
        /// will use the results from this operation.
        /// </summary>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <remarks>Not all types of files support this operation.
        /// See the documentation for a list of supported file types. If you are doing
        /// multiple classifications or extractions with different configurations
        /// it is most efficient to call this method first, so the document is only
        /// read once.
        /// </remarks>
        public async Task ReadAsync(CancellationToken cancellationToken = default)
        {
            var readUrl = _behaviours["document:read"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Put,
                readUrl.CreateUri());

            await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the results of a document read and writes them in the requested format
        /// to the specified path.
        /// </summary>
        /// <param name="path">The path of the file to write the results to</param>
        /// <param name="format">The format of the results required</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <remarks>The Read method must be called before this method, otherwise a
        /// WaivesApiException will be thrown.
        /// </remarks>
        public async Task GetReadResultsAsync(string path, ReadResultsFormat format, CancellationToken cancellationToken = default)
        {
            using (var fileStream = File.OpenWrite(path))
            {
                await GetReadResultsAsync(fileStream, format, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the results of a document read and writes them in the requested format
        /// to the specified path.
        /// </summary>
        /// <param name="resultsStream">The stream to write the results to</param>
        /// <param name="format">The format of the results required</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <remarks>The Read method must be called before this method, otherwise a
        /// WaivesApiException will be thrown.
        /// </remarks>
        public async Task GetReadResultsAsync(Stream resultsStream, ReadResultsFormat format, CancellationToken cancellationToken = default)
        {
            var readUrl = _behaviours["document:read"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Get,
                readUrl.CreateUri(),
                new Dictionary<string, string>
                {
                    { "Accept", format.ToMimeType() }
                });

            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response
                .Content
                .ReadAsStreamAsync()
                .ConfigureAwait(false);

            await responseBody
                .CopyToAsync(resultsStream)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Performs classification on this document using the given classifier
        /// name. The named classifier must already exist in the Waives platform.
        /// </summary>
        /// <param name="classifierName">The name of the classifier to use.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>The results of the classification operation.</returns>
        public async Task<ClassificationResult> ClassifyAsync(string classifierName, CancellationToken cancellationToken = default)
        {
            var classifyUrl = _behaviours["document:classify"];

            var request = new HttpRequestMessageTemplate(HttpMethod.Post,
                classifyUrl.CreateUri(new
                {
                    classifier_name = classifierName
                }));

            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsAsync<ClassificationResponse>().ConfigureAwait(false);
            return responseBody.ClassificationResults;
        }

        /// <summary>
        /// Performs extraction on this document using the given extractor
        /// name. The named extractor must already exist in the Waives platform.
        /// </summary>
        /// <param name="extractorName">The name of the extractor to use.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>The results of the extraction operation.</returns>
        public async Task<ExtractionResults> ExtractAsync(string extractorName, CancellationToken cancellationToken = default)
        {
            var extractionResponse = await DoExtractionAsync(
                    extractorName,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return await extractionResponse.ReadAsAsync<ExtractionResults>().ConfigureAwait(false);
        }

        /// <summary>
        /// Performs redaction on this document using results from an
        /// <see cref="ExtractAsync" langword="extraction"/> operation using the
        /// specified extractor name. The named extractor must already exist in
        /// the Waives platform. The result of this operation is a PDF file with
        /// the extracted data removed from the file.
        /// </summary>
        /// <param name="extractorName">
        /// The name of the extractor to use to identify the areas of the
        /// document to redact.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Stream"/> of bytes containing a PDF file. The returned
        /// Stream should be disposed of in your own code.
        /// </returns>
        public async Task<Stream> RedactAsync(string extractorName, CancellationToken cancellationToken = default)
        {
            var extractionResponse =
                await DoExtractionAsync(extractorName, RedactionRequest.MimeType, cancellationToken)
                .ConfigureAwait(false);

            var redactions = await extractionResponse.ReadAsStringAsync().ConfigureAwait(false);
            var request = new HttpRequestMessageTemplate(HttpMethod.Post, new Uri($"/documents/{Id}/redact", UriKind.Relative))
            {
                Content = new JsonContent(redactions)
            };

            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        private async Task<HttpContent> DoExtractionAsync(string extractorName,
            string desiredResponseFormat = ExtractionResults.MimeType,
            CancellationToken cancellationToken = default)
        {
            var extractUrl = _behaviours["document:extract"].CreateUri(new
            {
                extractor_name = extractorName
            });

            var request = new HttpRequestMessageTemplate(
                HttpMethod.Post,
                extractUrl,
                new Dictionary<string, string>
                {
                    { "Accept", desiredResponseFormat }
                });

            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response.Content;
        }
    }
}