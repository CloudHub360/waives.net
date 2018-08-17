using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Waives.Reactive;

namespace Waives.Extensions.DocumentChannels.Filesystem
{
    public sealed class FileSystemDocumentEmitter : DocumentEmitter, IDisposable
    {
        private readonly FileSystemWatcher _filesystemWatcher;

        public FileSystemDocumentEmitter(string path)
        {
            _filesystemWatcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
        }

        protected override Task EmitDocuments(CancellationToken token)
        {
            // Identify and emit existing files in the directory
            var files = Directory
                .EnumerateFiles(_filesystemWatcher.Path)
                .Select(p => new FileSystemDocument(p))
                .ToList();

            Console.WriteLine($"Found {files.Count} files to process in {_filesystemWatcher.Path}");
            files.ForEach(EmitDocument);

            // Emit new documents as they are created
            _filesystemWatcher.Created += (s, e) => { EmitDocument(new FileSystemDocument(e.FullPath)); };

            // Complete this emitter when the token is cancelled
            token.Register(Completed);

            return Task.CompletedTask;
        }

        public override bool EnableRaisingEvents
        {
            get => _filesystemWatcher.EnableRaisingEvents;
            set => _filesystemWatcher.EnableRaisingEvents = value;
        }

        public void Dispose()
        {
            Completed();
            _filesystemWatcher.Dispose();
        }
    }
}