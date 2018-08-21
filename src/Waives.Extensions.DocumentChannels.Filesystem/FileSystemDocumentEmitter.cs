using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Waives.Reactive;

namespace Waives.Extensions.DocumentChannels.Filesystem
{
    /// <inheritdoc cref="DocumentEmitter" />
    /// <summary>
    /// Watches the provided path for new files being created and emits them via
    /// the <see cref="DocumentEmitter.NewDocument"/> event.
    /// </summary>
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

        /// <inheritdoc />
        /// <remarks>
        /// Emits any existing files in the watched directory before subscribing to
        /// <see cref="FileSystemWatcher.Created"/> to emit notifications of new
        /// documents via <see cref="DocumentEmitter.EmitDocument"/>. Ensures this
        /// emitter is completed when the <paramref name="token"/> is cancelled.
        /// </remarks>
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

        /// <inheritdoc />
        /// <remarks>
        /// Enable or disable the underlying <see cref="FileSystemWatcher"/>.
        /// </remarks>
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