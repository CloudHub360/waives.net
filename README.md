# waives.net

This repository is where we develop the .NET SDK libraries for [Waives](https://waives.io/).
The .NET SDK targets .NET Standard 2.0. We explicitly support .NET Core 2.1 and
higher, as well as .NET Framework 4.7.2 and higher. The SDK _should_ work on
Mono version 5.4 and higher, but this is not tested and support is on a best-
effort basis only.

If you are using Visual Studio, please ensure you are using Visual Studio 2017
or later.

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
       <add key="Waives Pre-release" value="https://www.myget.org/F/waives-nightly/api/v3/index.json" protocolVersion="3" />
     </packageSources>
   </configuration>
   ```

3. Install the latest pre-release version using your NuGet client or

   ```powershell
   Install-Package -Pre Waives.Pipelines
   ```

## API overview

### Waives.Pipelines

Waives.Pipelines is the high-level document-processing API built against Waives.
It exposes a pipeline model sourcing documents from the place you specify (e.g.
file system, cloud storage, database, etc.), defines a set of operations that
can be completed on a document, such as classification and extraction, and
provides hooks for running your own actions with a document at arbitrary points
in the pipeline's execution. The pipeline API is a fluent API defining the
pipeline's execution order.

To get started with this API, simply:

```csharp
// Define a pipeline that classifies each document and writes its classification
// to the console. This will also authenticate with the Waives API.
var pipeline = await WaivesApi.CreatePipeline(new WaivesOptions
{
    ClientId = "clientId",
    ClientSecret = "clientSecret"
});

pipeline.WithDocumentsFrom(myDocumentSource)
    .ClassifyWith("mortgages")
    .Then(d => Console.WriteLine(d.ClassificationResults.DocumentType))
    .OnPipelineCompleted(() => Console.WriteLine("Classification complete"));

try
{
    // Run the pipeline
    pipeline.Start();
}
catch (PipelineException ex)
{
    // Handle an unrecoverable pipeline processing error
    Console.WriteLine(ex);
}
```

### Waives.Http

Waives.Http is a lower-level client which makes the HTTP requests against the
Waives API. This is provided for more advanced scenarios which do not fit the
pipeline model so cleanly.

To get started with this API:

```csharp
var client = WaivesClient.Create();
await client.Login("clientId", "clientSecret");

try
{
    var document = await client.CreateDocument(@"C:\path\to\my\document.pdf");
    var classification = await document.Classify("mortgages");
}
finally
{
    // Ensure the document is deleted after use
    await document.Delete();
}
```

### Extension packages

Similarly to the ASP.NET Core set of packages, we publish non-core functionality
as separate NuGet packages named extensions. These extensions are:

* **Waives.Pipelines.Extensions.DocumentSources.FileSystem**: provides an implementation of
  `Document` and `DocumentEmitter` reading files from a local or remote disk.

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
