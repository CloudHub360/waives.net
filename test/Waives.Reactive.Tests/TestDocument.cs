using System.IO;
using System.Threading.Tasks;

namespace Waives.Reactive.Tests
{
    internal class TestDocument : Document
    {
        public TestDocument(byte[] contents) : base("Test Document")
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