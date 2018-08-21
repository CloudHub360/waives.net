using System.IO;
using System.Threading.Tasks;

namespace Waives.Pipelines.Tests
{
    internal class TestDocument : Document
    {
        public TestDocument(byte[] contents, string sourceId = "Test Document") : base(sourceId)
        {
            Stream = new MemoryStream(contents);
        }

        public override Task<Stream> OpenStream()
        {
            return Task.FromResult(Stream);
        }

        internal Stream Stream { get; }
    }
}