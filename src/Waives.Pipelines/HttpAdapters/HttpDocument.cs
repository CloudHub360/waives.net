using System.IO;
using System.Runtime.CompilerServices;
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
        Task<ClassificationResult> ClassifyAsync(string classifierName);

        Task<ExtractionResults> ExtractAsync(string extractorName);

        Task<Stream> RedactAsync(string extractorName);

        Task DeleteAsync();

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

        public async Task<ClassificationResult> ClassifyAsync(string classifierName)
        {
            return await _documentClient.ClassifyAsync(classifierName).ConfigureAwait(false);
        }

        public async Task<ExtractionResults> ExtractAsync(string extractorName)
        {
            return await _documentClient.ExtractAsync(extractorName).ConfigureAwait(false);
        }

        public async Task<Stream> RedactAsync(string extractorName)
        {
            return await _documentClient.RedactAsync(extractorName).ConfigureAwait(false);
        }

        public async Task DeleteAsync()
        {
            await _documentClient.DeleteAsync().ConfigureAwait(false);
        }
    }
}