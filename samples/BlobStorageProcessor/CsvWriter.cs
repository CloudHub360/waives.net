using System;
using System.IO;
using CsvHelper;
using Waives.Http.Responses;
using Waives.Reactive;

namespace BlobStorageProcessor
{
    internal sealed class CsvWriter : IDisposable
    {
        private readonly IWriter _writer;

        public CsvWriter(TextWriter textWriter)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            _writer = new Factory().CreateWriter(textWriter);
            _writer.WriteHeader<ClassificationResult>();
            _writer.NextRecord();
        }

        public void Write(WaivesDocument value)
        {
            _writer.WriteRecord(value.ClassificationResults);
            _writer.NextRecord();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}