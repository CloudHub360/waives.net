using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Waives.Http.Responses;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Waives.Pipelines.HttpAdapters
{
    /// <summary>
    /// Adapter interface to allow us to substitute the underlying calls.
    /// </summary>
    internal interface IHttpDocument
    {
        Task<ClassificationResult> ClassifyAsync(string classifierName, CancellationToken cancellationToken = default);

        Task<ExtractionResults> ExtractAsync(string extractorName, CancellationToken cancellationToken = default);

        Task<Stream> RedactAsync(string extractorName, CancellationToken cancellationToken = default);

        Task DeleteAsync(CancellationToken cancellationToken = default);

        string Id { get; }
    }

    /// <summary>
    /// Adapter class to provide a live implementation of <see cref="IHttpDocument"/>
    /// with <see cref="Waives.Http.Document"/>. Every method on this class should
    /// simply delegate to the matching method on <see cref="Waives.Http.Document"/>,
    /// it should not implement any additional logic.
    /// </summary>
    internal class HttpDocument : IHttpDocument
    {
        private readonly Http.Document _documentClient;

        public string Id => _documentClient.Id;

        internal HttpDocument(Http.Document documentClient)
        {
            _documentClient = documentClient;
        }

        public async Task<ClassificationResult> ClassifyAsync(string classifierName, CancellationToken cancellationToken = default)
        {
            return await _documentClient
                .ClassifyAsync(classifierName, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<ExtractionResults> ExtractAsync(string extractorName, CancellationToken cancellationToken = default)
        {
            return await _documentClient
                .ExtractAsync(extractorName, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Stream> RedactAsync(string extractorName, CancellationToken cancellationToken = default)
        {
            return await _documentClient
                .RedactAsync(extractorName, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            await _documentClient
                .DeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}