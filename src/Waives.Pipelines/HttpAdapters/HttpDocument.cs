using System;
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
        Task<ClassificationResult> Classify(string classifierName);

        Task<ExtractionResults> Extract(string extractorName);

        Task Delete();

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

        public async Task<ClassificationResult> Classify(string classifierName)
        {
            return await _documentClient.Classify(classifierName).ConfigureAwait(false);
        }

        public async Task<ExtractionResults> Extract(string extractorName)
        {
            return await _documentClient.Extract(extractorName).ConfigureAwait(false);
        }

        public async Task Delete()
        {
            await _documentClient.Delete().ConfigureAwait(false);
        }
    }
}