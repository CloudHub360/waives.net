using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waives.Http.Responses;
using Waives.Pipelines.HttpAdapters;

namespace Waives.Pipelines
{
    /// <summary>
    /// Represents a document within the Waives API.
    /// </summary>
    public class WaivesDocument
    {
        private readonly IHttpDocument _waivesDocument;

        internal WaivesDocument(Document source, IHttpDocument waivesDocument)
        {
            Source = source;
            _waivesDocument = waivesDocument;
        }

        public string Id => _waivesDocument.Id;

        public Document Source { get; }

        internal IHttpDocument HttpDocument => _waivesDocument;

        public ClassificationResult ClassificationResults { get; private set; }

        public ExtractionResults ExtractionResults { get; private set; }

        public async Task<WaivesDocument> ClassifyAsync(string classifierName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(classifierName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(classifierName));
            }

            return new WaivesDocument(Source, _waivesDocument)
            {
                ClassificationResults = await _waivesDocument
                    .ClassifyAsync(classifierName, cancellationToken)
                    .ConfigureAwait(false),
                ExtractionResults = ExtractionResults
            };
        }

        public async Task<WaivesDocument> ExtractAsync(string extractorName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(extractorName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(extractorName));
            }

            return new WaivesDocument(Source, _waivesDocument)
            {
                ClassificationResults = ClassificationResults,
                ExtractionResults = await _waivesDocument
                    .ExtractAsync(extractorName, cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        public async Task<WaivesDocument> RedactAsync(
            string extractorName,
            Func<WaivesDocument, Stream, Task> resultFunc,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(extractorName))
            {
                throw new ArgumentNullException(nameof(extractorName));
            }

            if (resultFunc == null)
            {
                throw new ArgumentNullException(nameof(resultFunc));
            }

            var resultStream = await _waivesDocument
                .RedactAsync(extractorName, cancellationToken)
                .ConfigureAwait(false);
            await resultFunc(this, resultStream).ConfigureAwait(false);

            return this;
        }

        public async Task DeleteAsync()
        {
            await _waivesDocument.DeleteAsync().ConfigureAwait(false);
        }
    }
}