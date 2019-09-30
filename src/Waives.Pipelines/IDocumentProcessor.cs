using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal interface IDocumentProcessor
    {
        Task RunAsync(Document document, CancellationToken cancellationToken = default);
    }
}