using System;
using System.IO;
using Waives.Pipelines;
using Waives.Pipelines.Extensions.DocumentSources.FileSystem;

namespace FileSorter
{
    public class FileSorter
    {
        private readonly string _outboxPath;
        private readonly string _errorboxPath;

        public FileSorter(string outboxPath, string errorboxPath)
        {
            _outboxPath = outboxPath;
            _errorboxPath = errorboxPath;
        }

        public void MoveDocument(WaivesDocument result)
        {
            if (!(result.Source is FileSystemDocument document))
            {
                throw new InvalidOperationException("Cannot move a document which did not originate from the file system.");
            }

            MoveFile(document, _outboxPath, result.ClassificationResults.DocumentType);
        }

        private static void MoveFile(FileSystemDocument fileSystemDocument, string boxPath, string subfolder = "")
        {
            var destination = Path.Combine(boxPath, subfolder);
            EnsureDirectoryExists(destination);

            try
            {
                var destinationFileName = Path.Combine(destination, fileSystemDocument.FilePath.Name);
                Console.WriteLine($"Moving {fileSystemDocument} to {destinationFileName}");
                File.Move(fileSystemDocument.FilePath.FullName, destinationFileName);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Could not move file {fileSystemDocument.FilePath.FullName}: {ex.Message}");
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void HandleFailure(DocumentError error)
        {
            Console.WriteLine($"Processing {error.Document} failed: {error.Exception.Message}");

            if (!(error.Document is FileSystemDocument document))
            {
                throw new InvalidOperationException("Cannot move a document which did not originate from the file system.");
            }

            // Move the document to the error box
            MoveFile(document, _errorboxPath);

            // Write a log for the document indicating the failure.
            var logFile = Path.Combine(_errorboxPath, $"{document.FilePath.Name}.log");
            Console.WriteLine($"Writing error log file to {logFile}");
            File.WriteAllText(logFile, error.Exception.Message);
        }
    }
}
