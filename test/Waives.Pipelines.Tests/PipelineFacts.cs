using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Waives.Http.Responses;
using Waives.Pipelines.HttpAdapters;
using Xunit;

namespace Waives.Pipelines.Tests
{
    public class PipelineFacts
    {
        private readonly IHttpDocumentFactory _documentFactory =
            Substitute.For<IHttpDocumentFactory>();

        private readonly Pipeline _sut;
        private readonly IHttpDocument _httpDocument;

        public PipelineFacts()
        {
            _httpDocument = Substitute.For<IHttpDocument>();
            _httpDocument.Classify(Arg.Any<string>()).Returns(new ClassificationResult());
            _httpDocument.Extract(Arg.Any<string>()).Returns(new ExtractionResults());
            _httpDocument.Redact(Arg.Any<string>()).Returns(new MemoryStream(new byte[] { 1, 2, 3 }));
            _httpDocument.Delete();

            _documentFactory
                .CreateDocument(Arg.Any<Document>())
                .Returns(_httpDocument);

            _sut = new Pipeline(_documentFactory, 10);
        }

        [Fact]
        public async Task OnPipelineCompleted_is_run_at_the_end_of_a_successful_pipeline()
        {
            var pipelineCompleted = false;
            _sut.OnPipelineCompleted(() => pipelineCompleted = true);

            await _sut.RunAsync();

            Assert.True(pipelineCompleted);
        }

        [Fact]
        public async Task WithDocumentsFrom_creates_each_document_with_Waives()
        {
            var testDocument = new TestDocument(Generate.Bytes());
            var source = Observable.Return(testDocument);

            var pipeline = _sut.WithDocumentsFrom(source);
            await pipeline.RunAsync();

            await _documentFactory.Received(1).CreateDocument(testDocument);
        }

        [Fact]
        public async Task ClassifyWith_classifies_each_document_with_Waives()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var classifierName = Generate.String();

            var pipeline = _sut
                .WithDocumentsFrom(source)
                .ClassifyWith(classifierName)
                .Then(t => { Assert.NotNull(t.ClassificationResults); });

            await pipeline.RunAsync();

            await _httpDocument.Received(1).Classify(classifierName);
        }

        [Fact]
        public async Task ExtractWith_extracts_from_each_document_with_Waives()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var extractorName = Generate.String();

            var pipeline = _sut
                .WithDocumentsFrom(source)
                .ExtractWith(extractorName)
                .Then(t => { Assert.NotNull(t.ExtractionResults); });

            await pipeline.RunAsync();
            await _httpDocument.Received(1).Extract(extractorName);
        }

        [Fact]
        public async Task RedactWith_extracts_from_each_document_with_Waives()
        {
            var source = Observable.Return(new TestDocument(Generate.Bytes()));
            var extractorName = Generate.String();

            var pipeline = _sut
                .WithDocumentsFrom(source)
                .RedactWith(extractorName, redactedPdf =>
                {
                    AssertStreamsAreEqual(new MemoryStream(new byte[] { 1, 2, 3 }), redactedPdf);
                    return Task.CompletedTask;
                });

            await pipeline.RunAsync();
            await _httpDocument.Received(1).Redact(extractorName);
        }

        [Fact]
        public async Task
            Results_are_preserved_when_performing_classification_and_extraction_in_the_same_pipeline()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);

            await _sut
                .WithDocumentsFrom(source)
                .ClassifyWith(Generate.String())
                .ExtractWith(Generate.String())
                .Then(t =>
                {
                    Assert.NotNull(t.ExtractionResults);
                    Assert.NotNull(t.ClassificationResults);
                })
                .RunAsync();

            // Extract and classify in opposite order to ensure the results
            // are preserved in both directions
            await _sut
                .WithDocumentsFrom(source)
                .ExtractWith(Generate.String())
                .ClassifyWith(Generate.String())
                .Then(t =>
                {
                    Assert.NotNull(t.ExtractionResults);
                    Assert.NotNull(t.ClassificationResults);
                })
                .RunAsync();
        }

        [Fact]
        public async Task Then_invokes_the_supplied_Action()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var actionInvoked = false;

            var pipeline = _sut.WithDocumentsFrom(source)
                .Then(d => actionInvoked = true);

            await pipeline.RunAsync();

            Assert.True(actionInvoked);
        }

        [Fact]
        public async Task Then_invokes_the_supplied_async_Action()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var completion = new TaskCompletionSource<bool>();

            var pipeline = _sut.WithDocumentsFrom(source)
                .Then(async d => await Task.Run(() => completion.SetResult(true)));

            await pipeline.RunAsync();
            var actionWasCalled = await completion.Task;

            Assert.True(actionWasCalled);
        }

        [Fact]
        public async Task Then_invokes_the_supplied_Func()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var completion = new TaskCompletionSource<bool>();

            var pipeline = _sut.WithDocumentsFrom(source)
                .Then(d =>
                {
                    completion.SetResult(true);
                    return Task.FromResult(d);
                });

            await pipeline.RunAsync();
            var funcWasCalled = await completion.Task;

            Assert.True(funcWasCalled);
        }

        [Fact]
        public async Task The_document_is_deleted_when_processing_completes()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            await _sut
                .WithDocumentsFrom(source)
                .Then(d => d.HttpDocument.Received(1).Delete())
                .RunAsync();
        }

        [Fact]
        public async Task OnDocumentError_is_run_when_a_document_has_a_processing_error()
        {
            var onDocumentErrorActionRun = false;
            var document = new TestDocument(Generate.Bytes());
            var exception = new Exception("An exception");
            var source = Observable.Repeat(document, 1);

            await _sut.WithDocumentsFrom(source)
                .Then(d => throw exception)
                .OnDocumentError(err =>
                {
                    Assert.Same(document, err.Document);
                    Assert.Same(exception, err.Exception);
                    onDocumentErrorActionRun = true;
                })
                .RunAsync();

            Assert.True(onDocumentErrorActionRun);
        }

        [Fact]
        public async Task OnDocumentError_is_run_when_a_document_has_an_error_during_creation()
        {
            var onDocumentErrorActionRun = false;
            var document = new TestDocument(Generate.Bytes());
            var exception = new Exception("Could not create document");
            var source = Observable.Repeat(document, 1);

            _documentFactory
                .CreateDocument(Arg.Any<Document>())
                .Throws(exception);

            await _sut.WithDocumentsFrom(source)
                .OnDocumentError(err =>
                {
                    Assert.Same(document, err.Document);
                    Assert.NotNull(err.Exception);
                    Assert.Null(err.Exception.InnerException);
                    Assert.Equal(err.Exception.Message, exception.Message);
                    onDocumentErrorActionRun = true;
                })
                .RunAsync();

            Assert.True(onDocumentErrorActionRun);
        }

        [Fact]
        public async Task RunAsync_throws_when_OnDocumentError_throws()
        {
            var document = new TestDocument(Generate.Bytes());
            var source = Observable.Repeat(document, 1);

            await Assert.ThrowsAsync<Exception>(() => _sut.WithDocumentsFrom(source)
                .Then(waivesDocument => throw new Exception())
                .OnDocumentError(err =>
                {
                    throw new Exception();
                })
                .RunAsync());
        }

        [Fact]
        public async Task OnDocumentError_is_run_multiple_times_in_correct_order_if_specified()
        {
            var onDocumentErrorCalledFor = new List<(DocumentError error, int sequence)>();
            var document = new TestDocument(Generate.Bytes());
            var exception = new Exception("Could not create document");
            var source = Observable.Repeat(document, 1);

            _documentFactory
                .CreateDocument(Arg.Any<Document>())
                .Throws(exception);

            await _sut.WithDocumentsFrom(source)
                .OnDocumentError(err =>
                {
                    onDocumentErrorCalledFor.Add(
                        (error: err, sequence: 1));
                })
                .OnDocumentError(err =>
                {
                    onDocumentErrorCalledFor.Add(
                        (error: err, sequence: 2));
                })
                .RunAsync();

            Assert.Equal(2, onDocumentErrorCalledFor.Count);
            Assert.Equal(1, onDocumentErrorCalledFor.First().sequence);
            Assert.Equal(2, onDocumentErrorCalledFor.Last().sequence);
        }

        [Fact]
        public async Task RunAsync_throws_if_document_deletion_fails()
        {
            var expectedException = new Exception();
            _httpDocument.Delete().Throws(expectedException);
            var source = Observable.Return(new TestDocument(Generate.Bytes()));

            var pipeline = _sut
                .WithDocumentsFrom(source);
            var actualException = await Assert.ThrowsAsync<Exception>(() => pipeline.RunAsync());

            Assert.Same(expectedException, actualException);
        }
		
		private static void AssertStreamsAreEqual(Stream expected, Stream actual)
        {
            int expectedByte, actualByte;
            while ((expectedByte = expected.ReadByte()) != -1 &&
                   (actualByte = actual.ReadByte()) != -1)
            {
                Assert.Equal(expectedByte, actualByte);
            }
        }
    }
}