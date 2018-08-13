using System.IO;
using System.Threading.Tasks;

namespace Waives.Reactive.Tests
{
    internal class TestDocument : Document
    {
        public TestDocument() : base("Test Document")
        {
        }

        public override Task<Stream> OpenStream()
        {
            return Task.FromResult(Stream.Null);
        }
    }
}