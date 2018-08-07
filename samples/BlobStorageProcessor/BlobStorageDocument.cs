using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Waives.NET;

namespace BlobStorageProcessor
{
    internal class BlobStorageDocument : IDocumentSource
    {
        private readonly CloudBlockBlob _blob;

        public BlobStorageDocument(CloudBlockBlob blob)
        {
            _blob = blob;
        }

        public async Task<Stream> OpenStream()
        {
            return await _blob.OpenReadAsync();
        }

        public override string ToString()
        {
            return _blob.Name;
        }
    }
}