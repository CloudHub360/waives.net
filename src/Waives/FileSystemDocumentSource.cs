using System;
using System.IO;
using System.Threading.Tasks;

namespace Waives
{
    public class FileSystemDocumentSource : IDocumentSource, IEquatable<FileSystemDocumentSource>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as FileSystemDocumentSource);
        }

        public bool Equals(FileSystemDocumentSource other)
        {
            return Equals(this, other);
        }

        public static bool Equals(FileSystemDocumentSource x, FileSystemDocumentSource y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return string.Equals(x._filePath, y._filePath);
        }

        public override int GetHashCode()
        {
            return _filePath.GetHashCode();
        }
    }
}