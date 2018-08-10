﻿using System.IO;
using System.Threading.Tasks;
using Waives.Reactive;

namespace Waives.Extensions.DocumentChannels.Filesystem
{
    /// <summary>
    /// Represents a document on disk
    /// </summary>
    public class FileSystemDocument : Document
    {
        public FileInfo FilePath => new FileInfo(SourceId);

        public FileSystemDocument(string filePath) : base(filePath)
        {
        }

        public override Task<Stream> OpenStream()
        {
            return Task.FromResult(File.OpenRead(FilePath.FullName) as Stream);
        }
    }
}