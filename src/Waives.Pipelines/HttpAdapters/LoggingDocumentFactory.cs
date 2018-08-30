using System.Threading.Tasks;
using Waives.Http.Logging;

namespace Waives.Pipelines.HttpAdapters
{
    /// <inheritdoc />
    internal class LoggingDocumentFactory : IHttpDocumentFactory
    {
        private readonly ILogger _logger;
        private readonly IHttpDocumentFactory _wrappedDocumentFactory;

        public LoggingDocumentFactory(ILogger logger, IHttpDocumentFactory underylingDocumentFactory)
        {
            _logger = logger;
            _wrappedDocumentFactory = underylingDocumentFactory;
        }

        public async Task<IHttpDocument> CreateDocument(Document source)
        {
            var httpDocument = await _wrappedDocumentFactory.CreateDocument(source).ConfigureAwait(false);

            _logger.Log(LogLevel.Info, $"Created Waives document {httpDocument.Id} from '{source.SourceId}'");

            return httpDocument;
        }
    }
}