using System;
using System.IO;
using System.Reactive.Linq;

namespace Waives.Extensions.DocumentChannels.Filesystem
{
    public class FileSystemDocumentChannel : DocumentChannel
    {
        private FileSystemDocumentChannel(IObservable<IDocumentSource> buffer) : base(buffer)
        {
        }

        public static DocumentChannel Create(string inbox, bool watch = false)
        {
            var initialContentsObservable = Directory
                .EnumerateFiles(inbox).ToObservable()
                .Select(p => new FileSystemDocumentSource(p));

            if (watch)
            {
                var watcher = new FileSystemWatcher(inbox)
                {
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };

                var inboxObservable = Observable
                    .FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Created))
                    .Select(e => new FileSystemDocumentSource(e.EventArgs.FullPath));

                return new FileSystemDocumentChannel(initialContentsObservable.Concat(inboxObservable));
            }

            return new FileSystemDocumentChannel(initialContentsObservable);
        }
    }
}