using System;
using System.IO;
using Waives;

namespace FileSorter
{
    public class FileSorter : IObserver<DocumentClassification>
    {
        private readonly string _outboxPath;
        private readonly string _errorboxPath;

        public FileSorter(string outboxPath, string errorboxPath)
        {
            _outboxPath = outboxPath;
            _errorboxPath = errorboxPath;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
            // Move to failures box
        }

        public void OnNext(DocumentClassification value)
        {
            if (!(value.Document is FileSystemDocument document))
            {
                throw new InvalidOperationException("Cannot move");
            }

            MoveFile(document, _outboxPath, value.ClassificationResult.DocumentType);
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
                OnError(ex);
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
