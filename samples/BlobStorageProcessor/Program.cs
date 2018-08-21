using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocoptNet;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Waives.Reactive;

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
      BlobStorageProcessor.exe classify (<container>...) --classifier=<classifier> [--output=<file>] (--connection-string=<connection-string> | --container-sas=<container-sas>)
      BlobStorageProcessor.exe (-h | --help)
      BlobStorageProcessor.exe --version

    Options:
      -h --help                                Show this screen
      --classifier=<classifier>                The classifier, previously created on Waives, with which the blobs will be classified.
      --connection-string=<connection-string>  A connection string to your Azure Blob Storage account. At least one <container> must be specified if using this option.
      --container-sas=<container-sas>          A Shared Access Signature (SAS) for an Azure Blob Storage container. <container> is ignored if using this option.
      --output=<file>                          The file to which the CSV records will be written. [default: stdout]

";

        public static async Task Main(string[] args)
        {
            // Point at a blob storage account
            var options = new Docopt().Apply(Usage, args, version: "Blob Storage processor sample app 1.0", exit: true);

            var connectionString = options["--connection-string"]?.ToString() ?? string.Empty;
            var containerSas = options["--container-sas"]?.ToString() ?? string.Empty;
            var containers = EnumerateContainers(connectionString, containerSas, options["<container>"].AsEnumerable(i => i.ToString()));

            // Identify all blobs in the specified containers
            var blobs = (await Task.WhenAll(containers.Select(c => c.GetBlobs()))).SelectMany(c => c);

            // Log in to Waives
            await WaivesApi.Login("clientId", "clientSecret");

            // Create an Enumerable of Waives documents from the blobs
            var blobStorageDocuments = blobs.Select(b => new BlobStorageDocument(b));

            // Create a document source emitting each document in turn
            var blobStorage = new EnumerableDocumentSource(blobStorageDocuments);

            var writer = CreateResultWriter(options["--output"].ToString());
            var pipeline = WaivesApi.CreatePipeline()
                .WithDocumentsFrom(blobStorage)
                .ClassifyWith(options["--classifier"].ToString())
                .Then(d => writer.Write(d));

            try
            {
                pipeline.Start();
            }
            catch (PipelineException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static IEnumerable<BlobStorageContainer> EnumerateContainers(string connectionString, string containerSas, IEnumerable<string> containerNames)
        {
            IEnumerable<BlobStorageContainer> containers;
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var client = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
                containers = containerNames.Select(n => new BlobStorageContainer(client.GetContainerReference(n)));
            }
            else
            {
                containers = new[] {new BlobStorageContainer(new CloudBlobContainer(new Uri(containerSas)))};
            }

            return containers;
        }

        private static CsvWriter CreateResultWriter(string output)
        {
            if (output != "stdout")
            {
                // Create or overwrite destination file and configure writer
                return new CsvWriter(new StreamWriter(File.Create(output)));
            }

            return new CsvWriter(Console.Out);
        }
    }
}
