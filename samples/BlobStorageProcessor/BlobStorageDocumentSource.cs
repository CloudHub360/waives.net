using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Waives;

namespace BlobStorageProcessor
{
    internal class BlobStorageDocumentSource : DocumentSource
    {
        private BlobStorageDocumentSource(IObservable<Document> buffer) : base(buffer)
        {
        }

        public static BlobStorageDocumentSource Create(IEnumerable<CloudBlockBlob> blobs)
        {
            return new BlobStorageDocumentSource(blobs.Select(b => new BlobStorageDocument(b)).ToObservable());
        }
    }
}