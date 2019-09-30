using System.IO;
using System.Threading.Tasks;

namespace Waives.Pipelines.Tests
{
    internal class TestDocument : Document
    {
        internal const string SourceIdString = "Test Document";

        public TestDocument(byte[] contents, string sourceId = SourceIdString) : base(sourceId)
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