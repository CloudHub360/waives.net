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
            if (!(value.Document is FileSystemDocumentSource documentSource))
            {
                throw new InvalidOperationException("Cannot move");
            }

            MoveFile(documentSource, _outboxPath, value.ClassificationResult.DocumentType);
        }

        private void MoveFile(FileSystemDocumentSource fileSystemDocumentSource, string boxPath, string subfolder = "")
        {
            var destination = Path.Combine(boxPath, subfolder);
            EnsureDirectoryExists(destination);

            try
            {
                File.Move(fileSystemDocumentSource.FilePath.FullName,
                    Path.Combine(destination, fileSystemDocumentSource.FilePath.Name));
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
