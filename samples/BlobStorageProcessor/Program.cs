using System;
using DocoptNet;

namespace BlobStorageProcessor
{
    class Program
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

        static void Main(string[] args)
        {
            // Point at a blob storage account
            var options = new Docopt().Apply(Usage, args, version: "Blob Storage processor sample app 1.0", exit: true);

            // Identify all blobs there
            // Classify the files
            // Write results to a CSV file


        }
    }
}
