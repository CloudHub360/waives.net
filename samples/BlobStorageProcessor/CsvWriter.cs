using System;
using System.IO;
using CsvHelper;
using Waives;

namespace BlobStorageProcessor
{
    internal sealed class CsvWriter : IObserver<DocumentClassification>, IDisposable
    {
        private readonly IWriter _writer;

        public CsvWriter(TextWriter textWriter)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            _writer = new Factory().CreateWriter(textWriter);
            _writer.WriteHeader<DocumentClassification>();
            _writer.NextRecord();
        }

        public void OnCompleted()
        {
            _writer.Dispose();
        }

        public void OnError(Exception error)
        {
            Console.Error.WriteLine(error);
            _writer.Dispose();
        }

        public void OnNext(DocumentClassification value)
        {
            _writer.WriteRecord(value);
            _writer.NextRecord();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}