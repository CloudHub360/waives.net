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
    }
}