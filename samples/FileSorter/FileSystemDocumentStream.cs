using System;
using System.IO;
using System.Reactive.Linq;
using Waives.NET;

namespace FileSorter
{
    public class FileSystemDocumentStream : DocumentStream
    {
        private FileSystemDocumentStream(IObservable<IDocumentSource> buffer) : base(buffer)
        {
        }

        public static DocumentStream Create(string inbox)
        {
            var watcher = new FileSystemWatcher(inbox)
            {
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            var inboxObservable = Observable
                .FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Created))
                .Select(e => new FileSystemDocumentSource(e.EventArgs.FullPath));

            var initialContentsObservable = Directory
                .EnumerateFiles(inbox).ToObservable()
                .Select(p => new FileSystemDocumentSource(p));

            return new FileSystemDocumentStream(inboxObservable.Merge(initialContentsObservable));
        }
    }
}