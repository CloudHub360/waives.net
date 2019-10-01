using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Waives.Pipelines;

namespace BlobStorageProcessor
{
    internal class BlobStorageDocument : Document
    {
        private readonly CloudBlockBlob _blob;

        public BlobStorageDocument(CloudBlockBlob blob) : base(blob.Uri.ToString())
        {
            _blob = blob;
        }

        public override async Task<Stream> OpenStream()
        {
            return await _blob.OpenReadAsync();
        }

        public override string ToString()
        {
            return _blob.Name;
        }
    }
}