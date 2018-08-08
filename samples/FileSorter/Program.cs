﻿using System;
using System.IO;
using System.Threading.Tasks;
using Waives;
using Waives.Client;
using Waives.Extensions.DocumentChannels.Filesystem;

namespace FileSorter
{
    public static class Program
    {
        private const string Usage = @"File system sorter sample app.

    This sample application is supplied with the Waives.NET SDK to illustrate how to integrate
    with Waives' classification functionality. It watches the specified directory, classifying
    each file in that directory (including new ones added whilst the app is running) using the
    specified classifier, and moving the files to a subdirectory within the output directory.
    The subdirectory is named for the type of the document Waives identifies in the file.

    Subdirectories of the watched directory are not processed. Documents which cannot be
    processed will be written to the specified failures directory, or by default to a directory
    named ""failures"" within the input directory. A log message is written to the failures
    directory with the file indicating the error encountered in processing that file.

    Usage:
      FileSorter.exe watch <directory> using <classifier> (-o <outdir> | --output=<outdir>) [-f <faildir> | --failures=<faildir>]
      FileSorter.exe (-h | --help)
      FileSorter.exe --version

    Options:
      -h --help                           Show this screen.
      -o <outdir>, --output=<outdir>      The directory into which files will be sorted.
      -f <faildir>, --failures=<faildir>  The directory into which failed documents will be moved.

    ";

        public static async Task Main(string[] args)
        {
            var options = new DocoptNet.Docopt().Apply(Usage, args, version: "File system sorter sample app 1.0", exit: true);

            var inbox = options["<directory>"].ToString();
            var outbox = options["--output"].ToString();
            var errorbox = options["--failures"]?.ToString()
                           ?? Path.Combine(options["<directory>"].ToString(), "failures");

            EnsureDirectoryExists(inbox);
            EnsureDirectoryExists(outbox);
            EnsureDirectoryExists(errorbox);

            var waives = new WaivesClient();
            await waives.Login("clientId", "clientSecret");
            var classifier = await waives.GetClassifier(options["<classifier>"].ToString());

            var documentChannel = FileSystemDocumentChannel.Create(inbox);
            var classificationResultChannel = new ClassificationResultChannel(classifier, documentChannel);

            documentChannel.Subscribe(new ConsoleObserver<IDocumentSource>("documents"));
            classificationResultChannel.Subscribe(new ConsoleObserver<DocumentClassification>("classification"));
            classificationResultChannel.Subscribe(new FileSorter(outbox, errorbox));

            Console.ReadLine();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
