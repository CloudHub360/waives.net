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
                var segment = await _containerReference.ListBlobsSegmentedAsync(string.Empty, useFlatBlobListing: true, BlobListingDetails.None, null, continuation, null, null);
                continuation = segment.ContinuationToken;

                allBlobs.AddRange(segment.Results.OfType<CloudBlockBlob>());
            } while (continuation != null);

            return allBlobs;
        }
    }
}