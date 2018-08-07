using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocoptNet;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Waives.Client;
using Waives.NET;

namespace BlobStorageProcessor
{
    public static class Program
    {
        private const string Usage = @"Blob Storage processor sample app.

    This sample application is supplied with the Waives.NET SDK to illustrate how to integrate
    with Waives' classification functionality. It reads all the blobs in a container within an
    Azure Blob Storage account, classifies each blob using the specified classifier, and
    writes the results out to a CSV file at the specified path. The classifier must have been
    previously created in your Waives account at https://dashboard.waives.io/.

    If using a Shared Access Signature (SAS) for connecting to Azure Blob Storage, this app
    will only be able to access the blobs in the container referenced by that SAS. If
    connecting with a storage account connection string, you may pass multiple container names
    for classification, but all containers will be classified using the one classifier
    specified.

    Only block blobs will be classified by this application; all other blob types in the
    container(s) will be ignored.

    Usage:
      BlobStorageProcessor.exe classify (<container>...) --classifier=<classifier> (--connection-string=<connection-string> | --container-sas=<container-sas>)
      BlobStorageProcessor.exe (-h | --help)
      BlobStorageProcessor.exe --version

    Options:
      -h --help                                Show this screen
      --classifier=<classifier>                The classifier, previously created on Waives, with which the blobs will be classified.
      --connection-string=<connection-string>  A connection string to your Azure Blob Storage account. At least one <container> must be specified if using this option.
      --container-sas=<container-sas>          A Shared Access Signature (SAS) for an Azure Blob Storage container. <container> is ignored if using this option.

";

        public static async Task Main(string[] args)
        {
            // Point at a blob storage account
            var options = new Docopt().Apply(Usage, args, version: "Blob Storage processor sample app 1.0", exit: true);

            IEnumerable<CloudBlobContainer> containers;
            if (options["--connection-string"] != null)
            {
                var connectionString = options["--connection-string"].ToString();
                var containerNames = options["<container>"].AsEnumerable(i => i.ToString());

                var client = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
                containers = containerNames.Select(n => client.GetContainerReference(n));
            }
            else
            {
                var containerSas = options["--container-sas"].ToString();
                containers = new[] { new CloudBlobContainer(new Uri(containerSas)) };
            }

            // Identify all blobs there
            var blobs = (await Task.WhenAll(containers.Select(GetBlobsInContainer))).SelectMany(c => c);

            // Log in to Waives
            var waives = new WaivesClient();
            await waives.Login("clientId", "clientSecret");
            var classifier = await waives.GetClassifier(options["--classifier"].ToString());

            // Classify the files
            var stream = BlobStorageDocumentStream.Create(blobs);
            stream.SubscribeConsole("Blob Storage");
            var classificationResults = new ClassificationResultStream(classifier, stream);
            classificationResults.SubscribeConsole("classification");

            // Write results to a CSV file
        }

        private static async Task<IEnumerable<CloudBlockBlob>> GetBlobsInContainer(CloudBlobContainer container)
        {
            BlobContinuationToken continuation = null;
            var allBlobs = new List<CloudBlockBlob>();

            do
            {
                var segment = await container.ListBlobsSegmentedAsync(continuation);
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
