using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Waives.Reactive;

namespace Waives.Extensions.DocumentChannels.Filesystem
{
    /// <summary>
    /// A folder on disk from where documents will be retrieved for processing.
    /// </summary>
    public class FileSystemDocumentSource : DocumentSource
    {
        private FileSystemDocumentSource(IObservable<Document> buffer) : base(buffer)
        {
        }

        public static DocumentSource Create(string inbox, bool watch = false)
        {
            var initialContents = Directory
                .EnumerateFiles(inbox)
                .Select(p => new FileSystemDocument(p))
                .ToObservable();

            if (watch)
            {
                var watcher = new FileSystemWatcher(inbox)
                {
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };

                var createdFiles = Observable
                    .FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Created))
                    .Select(e => new FileSystemDocument(e.EventArgs.FullPath));

                return new FileSystemDocumentSource(initialContents.Concat(createdFiles));
            }

            return new FileSystemDocumentSource(initialContents);
        }
    }
}