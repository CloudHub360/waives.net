using System.Linq;
using System.Threading;
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
        Task<IHttpDocument> CreateDocumentAsync(Document source, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of <see cref="IHttpDocumentFactory"/>, creating
    /// documents in the Waives API.
    /// </summary>
    internal class HttpDocumentFactory : IHttpDocumentFactory
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly WaivesClient _apiClient;

        private HttpDocumentFactory(WaivesClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IHttpDocument> CreateDocumentAsync(Document source, CancellationToken cancellationToken = default)
        {
            using (var documentStream = await source.OpenStream().ConfigureAwait(false))
            {
                var httpDocument = new HttpDocument(await _apiClient
                    .CreateDocumentAsync(documentStream, cancellationToken)
                    .ConfigureAwait(false));

                return httpDocument;
            }
        }

        internal static async Task<IHttpDocumentFactory> CreateAsync(
            WaivesClient apiClient,
            bool deleteOrphanedDocuments = true,
            CancellationToken cancellationToken = default)
        {
            if (deleteOrphanedDocuments)
            {
                await DeleteOrphanedDocumentsAsync(apiClient, cancellationToken).ConfigureAwait(false);
                Logger.Info("Deleted all Waives documents");
            }

            return new LoggingDocumentFactory(new HttpDocumentFactory(apiClient));
        }

        private static async Task DeleteOrphanedDocumentsAsync(WaivesClient apiClient, CancellationToken cancellationToken = default)
        {
            var orphanedDocuments = await apiClient.GetAllDocumentsAsync(cancellationToken).ConfigureAwait(false);
            await Task.WhenAll(orphanedDocuments.Select(d => d.DeleteAsync())).ConfigureAwait(false);
        }
    }
}