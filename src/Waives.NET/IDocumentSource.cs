using System.IO;
using System.Threading.Tasks;

namespace Waives.NET
{
    public interface IDocumentSource
    {
        Task<Stream> OpenStream();
    }
}