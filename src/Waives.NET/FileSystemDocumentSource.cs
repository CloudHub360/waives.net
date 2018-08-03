using System;
using System.IO;
using System.Threading.Tasks;

namespace Waives.NET
{
    public class FileSystemDocumentSource : IDocumentSource
    {
        private readonly string _filePath;

        public FileInfo FilePath => new FileInfo(_filePath);

        public FileSystemDocumentSource(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            _filePath = filePath;
        }

        public Task<Stream> OpenStream()
        {
            return Task.FromResult(File.OpenRead(_filePath) as Stream);
        }
    }
}