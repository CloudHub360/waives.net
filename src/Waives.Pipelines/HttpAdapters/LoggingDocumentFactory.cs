using System.Threading.Tasks;
using Waives.Http.Logging;

namespace Waives.Pipelines.HttpAdapters
{
    /// <inheritdoc />
    internal class LoggingDocumentFactory : IHttpDocumentFactory
    {
        private readonly ILog _logger;
        private readonly IHttpDocumentFactory _wrappedDocumentFactory;

        public LoggingDocumentFactory(ILog logger, IHttpDocumentFactory underlyingDocumentFactory)
        {
            _logger = logger;
            _wrappedDocumentFactory = underlyingDocumentFactory;
        }

        public async Task<IHttpDocument> CreateDocument(Document source)
        {
            var httpDocument = await _wrappedDocumentFactory.CreateDocument(source).ConfigureAwait(false);

            _logger.Info($"Created Waives document {httpDocument.Id} from '{source.SourceId}'");

            return httpDocument;
        }
    }
}