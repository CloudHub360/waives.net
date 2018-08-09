using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace Waives.Extensions.DocumentChannels.Filesystem.Tests
{
    // ReSharper disable once InconsistentNaming
    public class FileSystemDocumentChannel_Should
    {
        private static readonly string TestDirectory = Path.Combine(Directory.GetCurrentDirectory(), "test-files");

        private readonly IEnumerable<string> _filesInTestDirectory;
        private readonly string _newTestFile = Path.Combine(TestDirectory, $"{Guid.NewGuid()}.txt");

        public FileSystemDocumentChannel_Should()
        {
            _filesInTestDirectory = Directory.EnumerateFiles(TestDirectory);
        }

        [Fact]
        public void Return_all_files_in_the_specified_directory()
        {
            var sut = FileSystemDocumentChannel.Create(TestDirectory);

            var expected = _filesInTestDirectory
                .Select(d => new FileSystemDocumentSource(d));

            Assert.Equal(expected, sut.ToEnumerable());
        }

        [Fact]
        public void Return_new_files_created_in_the_specified_directory()
        {
            var sut = FileSystemDocumentChannel.Create(TestDirectory, watch: true);

            sut.Cast<FileSystemDocumentSource>()
                .Skip(_filesInTestDirectory.Count())
                .Take(1)
                .Subscribe(d =>
                {
                    Assert.Equal(_newTestFile, d.FilePath.FullName);
                });

            File.Create(_newTestFile, 1, FileOptions.DeleteOnClose).Dispose();
        }
    }
}
