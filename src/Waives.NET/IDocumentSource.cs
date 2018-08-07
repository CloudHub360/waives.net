using System.IO;
using System.Threading.Tasks;

namespace Waives
{
    public interface IDocumentSource
    {
        Task<Stream> OpenStream();
    }
}