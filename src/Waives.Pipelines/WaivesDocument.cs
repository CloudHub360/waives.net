using System;
using System.IO;
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

        public async Task<WaivesDocument> Classify(string classifierName)
        {
            return new WaivesDocument(Source, _waivesDocument)
            {
                ClassificationResults = await _waivesDocument.Classify(classifierName).ConfigureAwait(false),
                ExtractionResults = ExtractionResults
            };
        }

        public async Task<WaivesDocument> Extract(string extractorName)
        {
            return new WaivesDocument(Source, _waivesDocument)
            {
                ClassificationResults = ClassificationResults,
                ExtractionResults = await _waivesDocument.Extract(extractorName).ConfigureAwait(false)
            };
        }

        public async Task<WaivesDocument> Redact(string extractorName, Func<WaivesDocument, Stream, Task> resultFunc)
        {
            if (string.IsNullOrWhiteSpace(extractorName))
            {
                throw new ArgumentNullException(nameof(extractorName));
            }

            if (resultFunc == null)
            {
                throw new ArgumentNullException(nameof(resultFunc));
            }

            var resultStream = await _waivesDocument.Redact(extractorName);
            await resultFunc(this, resultStream);

            return this;
        }

        public async Task Delete()
        {
            await _waivesDocument.Delete().ConfigureAwait(false);
        }
    }
}