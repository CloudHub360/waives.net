using System.IO;
using System.Threading.Tasks;

namespace Waives.Pipelines.Tests
{
    internal class TestDocument : Document
    {
        internal const string SourceIdString = "Test Document";

        private readonly Stream _stream;

        public TestDocument(byte[] contents, string sourceId = SourceIdString) : base(sourceId)
        {
            _stream = new MemoryStream(contents);
        }

        public override Task<Stream> OpenStream()
        {
            return Task.FromResult(_stream);
        }
    }
}