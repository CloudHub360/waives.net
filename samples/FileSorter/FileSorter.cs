using System;
using System.IO;
using Waives.Extensions.DocumentChannels.Filesystem;
using Waives.Pipelines;

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

        private void MoveFile(FileSystemDocument fileSystemDocument, string boxPath, string subfolder = "")
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
    }
}
