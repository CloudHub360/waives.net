# waives.net

This repository is where we develop the .NET SDK libraries for [Waives](https://waives.io/).
The .NET SDK targets .NET Standard 2.0. We explicitly support .NET Core 2.1 and
higher, as well as .NET Framework 4.7.2 and higher. The SDK _should_ work on
Mono version 5.4 and higher, but this is not tested and support is on a best-
effort basis only.

If you are using Visual Studio, please ensure you are using Visual Studio 2017
or later.

## Installing the SDK

The Waives .NET SDK is available as a set of NuGet packages. You can install them in the usual
way, detailed below for stable builds and early releases. [The documentation](https://docs.waives.io/docs/dotnet)
has information to help you get started, and you will need to obtain an API client ID and
secret from [the Waives dashboard](https://dashboard.waives.io).

### Stable builds

These are available from the public NuGet feed, and can be installed using one of the following
mechanisms, as best suits your application.

#### Package Manager
```powershell
Install-Package Waives.Pipelines
```

#### dotnet CLI
```powershell
dotnet add package Waives.Pipelines
```

#### `PackageReference` project file element
```xml
<PackageReference Include="Waives.Pipelines" Version="1.0.0" />
```

### Development builds
If you need a preview (pre-beta) release, you can pull the bleeding-edge versions of the libraries
from our MyGet feed, as described here.

#### Package Manager
```powershell
Install-Package -Pre Waives.Pipelines -Source https://www.myget.org/F/waives-nightly/api/v3/index.json
```

#### dotnet CLI
```powershell
dotnet add package Waives.Pipelines --source https://www.myget.org/F/waives-nightly/api/v3/index.json 
```

#### `PackageReference` project file element
Create a file called `NuGet.config` in the same location as your Visual Studio solution file, and
add the following content to it:
```xml
<?xml version="1.0" encoding="utf-8"?>                                                                                                                                                                                                                                          <configuration>
  <packageSources>
    <add key="Waives Pre-Release" value="https://www.myget.org/F/waives-nightly/api/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

Now update your project file to include the following line, replacing the version string with the
version you wish to install.
```xml
<PackageReference Include="Waives.Pipelines" Version="1.0.0" />
```

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
