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
    public static class FileSystem
    {
        /// <summary>
        /// Convenience method creating a <see cref="DocumentSource"/> watching the specified
        /// <paramref name="inbox"/> path for new files being created.
        /// </summary>
        /// <remarks>
        /// Any files already in existence in the given <paramref name="inbox"/> path will be
        /// included in the returned <see cref="DocumentSource"/>.
        /// </remarks>
        /// <param name="inbox">The path to watch for new documents.</param>
        /// <param name="token">A token for cancelling the watch.</param>
        /// <returns>An <see cref="DocumentSource"/> emitting a new document as they are created
        /// in the watched path.</returns>
        public static DocumentSource WatchForChanges(string inbox, CancellationToken token)
        {
            return EventingDocumentSource.Create(
                new FileSystemDocumentEmitter(inbox),
                token);
        }

        /// <summary>
        /// Convenience method creating a <see cref="DocumentSource"/> enumerating all files
        /// in the specified <paramref name="path"/> directory.
        /// </summary>
        /// <param name="path">The path to enumerate for files.</param>
        /// <returns>A <see cref="DocumentSource"/> emitting documents for the enumerated
        /// files.</returns>
        public static DocumentSource ReadFrom(string path)
        {
            return new EnumerableDocumentSource(
                Directory.EnumerateFiles(path)
                    .Select(f => new FileSystemDocument(f)));
        }
    }
}