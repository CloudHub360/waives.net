using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Waives;

namespace BlobStorageProcessor
{
    internal class BlobStorageDocumentStream : DocumentStream
    {
        private BlobStorageDocumentStream(IObservable<IDocumentSource> buffer) : base(buffer)
        {
        }

        public static BlobStorageDocumentStream Create(IEnumerable<CloudBlockBlob> blobs)
        {
            return new BlobStorageDocumentStream(blobs.Select(b => new BlobStorageDocument(b)).ToObservable());
        }
    }
}