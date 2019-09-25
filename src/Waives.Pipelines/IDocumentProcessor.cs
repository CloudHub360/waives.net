using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal interface IDocumentProcessor
    {
        Task Run(Document doc);
    }
}