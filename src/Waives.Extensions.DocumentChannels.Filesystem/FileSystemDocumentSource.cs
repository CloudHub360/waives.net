using System.IO;
using System.Linq;
using System.Threading;
using Waives.Reactive;

namespace Waives.Extensions.DocumentChannels.Filesystem
{
    /// <summary>
    /// Convenience class for creating <see cref="DocumentSource"/>s for paths
    /// on the file system.
    /// </summary>
    public static class FileSystemDocumentSource
    {
        /// <summary>
        /// Convenience factory method for creating a <see cref="DocumentSource"/> for the given
        /// path on a local or remote filesystem.
        /// </summary>
        /// <param name="inbox">The path containing the documents to process.</param>
        /// <param name="watch">Whether or not the path should be watched for new documents added
        /// to it.</param>
        /// <returns>An <see cref="EnumerableDocumentSource"/> for the path if <paramref name="watch"/>
        /// is <c>false</c>; otherwise an <see cref="EventingDocumentSource"/>.</returns>
        public static DocumentSource Create(string inbox, bool watch = false)
        {
            if (watch)
            {
                return EventingDocumentSource.Create(
                    new FileSystemDocumentEmitter(inbox),
                    CancellationToken.None);
            }

            return new EnumerableDocumentSource(
                Directory.EnumerateFiles(inbox)
                    .Select(path => new FileSystemDocument(path)));
        }
    }
}