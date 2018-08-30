using System;
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

        public ExtractionResponse ExtractionResults { get; private set; }

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

        public async Task Delete(Action afterDeletedAction)
        {
            await _waivesDocument.Delete(afterDeletedAction).ConfigureAwait(false);
        }
    }
}