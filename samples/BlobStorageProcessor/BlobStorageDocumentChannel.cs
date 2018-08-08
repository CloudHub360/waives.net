using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Waives;

namespace BlobStorageProcessor
{
    internal class BlobStorageDocumentChannel : DocumentChannel
    {
        private BlobStorageDocumentChannel(IObservable<IDocumentSource> buffer) : base(buffer)
        {
        }

        public static BlobStorageDocumentChannel Create(IEnumerable<CloudBlockBlob> blobs)
        {
            return new BlobStorageDocumentChannel(blobs.Select(b => new BlobStorageDocument(b)).ToObservable());
        }
    }
}