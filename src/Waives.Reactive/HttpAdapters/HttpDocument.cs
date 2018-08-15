using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waives.Http.Responses;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Waives.Reactive.HttpAdapters
{
    /// <summary>
    /// Adapter interface to allow us to substitute the underlying calls.
    /// </summary>
    internal interface IHttpDocument
    {
        Task<ClassificationResult> Classify(string classifierName);
        Task Delete(Action afterDeletedAction);
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

        internal HttpDocument(Http.Document documentClient)
        {
            _documentClient = documentClient;
        }

        public async Task<ClassificationResult> Classify(string classifierName)
        {
            return await _documentClient.Classify(classifierName).ConfigureAwait(false);
        }

        public async Task Delete(Action afterDeletedAction)
        {
            await _documentClient.Delete().ConfigureAwait(false);

            afterDeletedAction();
        }
    }
}