using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Waives.Pipelines.Extensions.DocumentSources.FileSystem;
using Xunit;

namespace Waives.Extensions.DocumentChannels.Filesystem.Tests
{
    public class FileSystemFacts
    {
        private static readonly string TestDirectory = Path.Combine(Directory.GetCurrentDirectory(), "test-files");

        private readonly IEnumerable<string> _filesInTestDirectory;
        private readonly string _newTestFile = Path.Combine(TestDirectory, $"{Guid.NewGuid()}.txt");

        public FileSystemFacts()
        {
            _filesInTestDirectory = Directory.EnumerateFiles(TestDirectory);
        }

        [Fact]
        public void Return_all_files_in_the_specified_directory()
        {
            var sut = FileSystem.ReadFrom(TestDirectory);

            var expected = _filesInTestDirectory
                .Select(d => new FileSystemDocument(d));

            Assert.Equal(expected, sut.ToEnumerable());
        }

        [Fact]
        public void Return_new_files_created_in_the_specified_directory()
        {
            var sut = FileSystem.WatchForChanges(TestDirectory, CancellationToken.None);

            sut.Cast<FileSystemDocument>()
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
