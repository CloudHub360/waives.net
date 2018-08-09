using System;
using System.IO;
using System.Linq;
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
            var initialContents = Directory
                .EnumerateFiles(inbox)
                .Select(p => new FileSystemDocumentSource(p))
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
                    .Select(e => new FileSystemDocumentSource(e.EventArgs.FullPath));

                return new FileSystemDocumentChannel(initialContents.Concat(createdFiles));
            }

            return new FileSystemDocumentChannel(initialContents);
        }
    }
}