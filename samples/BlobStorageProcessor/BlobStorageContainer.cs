using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobStorageProcessor
{
    internal class BlobStorageContainer
    {
        private readonly CloudBlobContainer _containerReference;

        public BlobStorageContainer(CloudBlobContainer containerReference)
        {
            _containerReference = containerReference ?? throw new ArgumentNullException(nameof(containerReference));
        }

        public async Task<IEnumerable<CloudBlockBlob>> GetBlobs()
        {
            BlobContinuationToken continuation = null;
            var allBlobs = new List<CloudBlockBlob>();

            do
            {
                var segment = await _containerReference.ListBlobsSegmentedAsync(continuation);
                continuation = segment.ContinuationToken;

                var directories = segment.Results.OfType<CloudBlobDirectory>();
                var blobsInDirectories = (await Task.WhenAll(directories.Select(GetBlobsInDirectory))).SelectMany(d => d);
                allBlobs.AddRange(segment.Results.OfType<CloudBlockBlob>().Concat(blobsInDirectories));
            } while (continuation != null);

            return allBlobs;
        }

        private static async Task<IEnumerable<CloudBlockBlob>> GetBlobsInDirectory(CloudBlobDirectory directory)
        {
            BlobContinuationToken continuation = null;
            var allBlobs = new List<CloudBlockBlob>();

            do
            {
                var blobSegment = await directory.ListBlobsSegmentedAsync(continuation);
                continuation = blobSegment.ContinuationToken;

                var directories = blobSegment.Results.OfType<CloudBlobDirectory>();
                var subDirectoryBlobs = (await Task.WhenAll(directories.Select(GetBlobsInDirectory))).SelectMany(d => d);

                allBlobs.AddRange(blobSegment.Results.OfType<CloudBlockBlob>().Concat(subDirectoryBlobs));
            } while (continuation != null);

            return allBlobs;
        }
    }
}