using System.Threading.Tasks;
using Waives.Http;

namespace Waives.Reactive.HttpAdapters
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

        internal HttpDocumentFactory(WaivesClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IHttpDocument> CreateDocument(Document source)
        {
            using (var documentStream = await source.OpenStream().ConfigureAwait(false))
            {
                return new HttpDocument(await _apiClient.CreateDocument(documentStream).ConfigureAwait(false));
            }
        }
    }
}