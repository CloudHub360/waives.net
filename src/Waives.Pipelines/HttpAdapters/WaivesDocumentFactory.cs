using System.Linq;
using System.Threading.Tasks;
using Waives.Http;
using Waives.Http.Logging;

namespace Waives.Pipelines.HttpAdapters
{
    /// <summary>
    /// Adapter interface for creating documents.
    /// </summary>
    internal interface IHttpDocumentFactory
    {
        Task<IHttpDocument> CreateDocument(Document source);
    }

    /// <summary>
    /// Default implementation of <see cref="IHttpDocumentFactory"/>, creating
    /// documents in the Waives API.
    /// </summary>
    internal class HttpDocumentFactory : IHttpDocumentFactory
    {
        private readonly WaivesClient _apiClient;

        private HttpDocumentFactory(WaivesClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IHttpDocument> CreateDocument(Document source)
        {
            using (var documentStream = await source.OpenStream().ConfigureAwait(false))
            {
                var httpDocument = new HttpDocument(await _apiClient.CreateDocument(documentStream).ConfigureAwait(false));
                _apiClient.Logger.Log(LogLevel.Info, $"Created Waives document {httpDocument.Id} from '{source.SourceId}'");
                return httpDocument;
            }
        }

        internal static async Task<HttpDocumentFactory> Create(WaivesClient apiClient, bool deleteOrphanedDocuments = true)
        {
            if (deleteOrphanedDocuments)
            {
                await DeleteOrphanedDocuments(apiClient).ConfigureAwait(false);
            }

            return new HttpDocumentFactory(apiClient);
        }

        private static async Task DeleteOrphanedDocuments(WaivesClient apiClient)
        {
            var orphanedDocuments = await apiClient.GetAllDocuments().ConfigureAwait(false);
            await Task.WhenAll(orphanedDocuments.Select(d => d.Delete())).ConfigureAwait(false);

            apiClient.Logger.Log(LogLevel.Info, "Deleted all Waives documents");
        }
    }
}