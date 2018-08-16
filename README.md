# waives.net

This repository is where we develop the .NET SDK libraries for [Waives](https://waives.io/).

## Getting Started

The Waives .NET SDK is available as a set of NuGet packages. You can download
the bleeding-edge version of the libraries from MyGet, which requires a little
bit of configuration of NuGet:

1. Add a [NuGet.config](https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior)
  file to the root of your project's repository.
2. Add the following content to this file:

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
     <configuration>
       <packageSources>
         <add key="Waives Pre-release" value="" protocolVersion="" />
       </packageSources>
     </configuration>
   ```

3. Set the `value` and `protocolVersion` attributes as follows:
   * **Visual Studio 2015+**:
     * `value`: https://www.myget.org/F/waives-nightly/api/v3/index.json
     * `protocolVersion`: 3
   * **Visual Studio 2012/2013**
     * `value`: https://www.myget.org/F/waives-nightly/api/v2
     * `protocolVersion`: 2
4. Install the latest pre-release version using your NuGet client or

   ```powershell
   Install-Package -Pre Waives.Reactive
   ```

## API overview

### Waives.Reactive

Waives.Reactive is the high-level document-processing API built against Waives.
It exposes a pipeline model sourcing documents from the place you specify (e.g.
file system, cloud storage, database, etc.), defines a set of operations that
can be completed on a document, such as classification and extraction, and
provides hooks for running your own actions with a document at arbitrary points
in the pipeline's execution. The pipeline API is a fluent API defining the
pipeline's execution order.

To get started with this API, simply:

```csharp
// Authenticate with the Waives API, using a client ID and secret from your
// account in the Waives Dashboard, https://dashboard.waives.io/.
await WaivesApi.Login("clientId", "clientSecret");

// Define a pipeline that classifies each document and writes its classification
// to the console
var pipeline = WaivesApi.CreatePipeline()
    .WithDocumentsFrom(myDocumentSource)
    .ClassifyWith("mortgages")
    .Then(d => Console.WriteLine(d.ClassificationResults.DocumentType))
    .OnPipelineCompleted(() => Console.WriteLine("Classification complete"));

try
{
    // Run the pipeline
    pipline.Start();
}
catch (PipelineException ex)
{
    // Handle an unrecoverable pipeline processing error
    Console.WriteLine(ex);
}
```

### Waives.Http

Waives.Http is a lower-level client which makes the HTTP requests against the
Waives API. This is provided for completeness for advanced scenarios that may
not fit the pipeline model nicely, but is generally considered an implementation
detail of Waives.Reactive.

### Extension packages

Similarly to the ASP.NET Core set of packages, we publish non-core functionality
as separate NuGet packages named extensions. These extensions are:

* **Waives.Extensions.DocumentChannels.Filesystem**: provides an implementation of
  `Document` and `DocumentSource` reading files from a local or remote disk.

## Sample applications

### File system sorter

The [File system sorter sample app](https://github.com/waives/waives.net/tree/master/samples/FileSorter)
watches a folder for any new files dropped into that folder, classifies them
using the specified [Waives classifer](https://docs.waives.io/docs/classification-overview),
and moves the files to the specified outbox, under a subfolder named for the
resulting document type from classification.

### Blob storage processor

The [Blob storage processor sample app](https://github.com/waives/waives.net/tree/master/samples/BlobStorageProcessor)
enumerates all blobs in the specified blob storage container(s), classifies them
using the specified [Waives classifer](https://docs.waives.io/docs/classification-overview),
and writes the results in CSV format to the console or to the specified destination file.