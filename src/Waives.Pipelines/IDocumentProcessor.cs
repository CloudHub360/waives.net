using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal interface IDocumentProcessor<T>
    {
        Task Run(T doc);
    }
}