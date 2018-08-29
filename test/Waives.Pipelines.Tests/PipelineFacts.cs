using System;
using System.Collections.Generic;
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
        private readonly IHttpDocumentFactory _documentFactory = Substitute.For<IHttpDocumentFactory>();
        private readonly Pipeline _sut;

        public PipelineFacts()
        {
            var httpDocument = Substitute.For<IHttpDocument>();
            httpDocument.Classify(Arg.Any<string>()).Returns(new ClassificationResult());
            httpDocument.Delete(Arg.Invoke());

            _documentFactory
                .CreateDocument(Arg.Any<Document>())
                .Returns(httpDocument);

            _sut = new Pipeline(_documentFactory, 10);
        }

        [Fact]
        public void OnPipelineCompleted_is_run_at_the_end_of_a_successful_pipeline()
        {
            var pipelineCompleted = false;
            _sut.OnPipelineCompleted(() => pipelineCompleted = true);

            _sut.Start();

            Assert.True(pipelineCompleted);
        }

//        [Fact]
//        public void WithDocumentsFrom_projects_the_rate_limited_documents_into_the_pipeline()
//        {
//            var expectedDocuments = Enumerable.Repeat<Document>(new TestDocument(Generate.Bytes()), 5).ToObservable();
//
//            var pipeline = _sut.WithDocumentsFrom(expectedDocuments);
//
//            Assert.Equal(
//                expectedDocuments.ToEnumerable(),
//                pipeline.Select(d => d.Source).ToEnumerable());
//        }

//        [Fact]
//        public void WithDocumentsFrom_creates_each_document_with_Waives()
//        {
//            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
//
//            var pipeline = _sut.WithDocumentsFrom(source);
//
//            pipeline.Subscribe(t =>
//            {
//                _documentFactory.Received(1).CreateDocument(t.Source);
//            });
//        }

//        [Fact]
//        public void ClassifyWith_classifies_each_document_with_Waives()
//        {
//            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
//            var classifierName = Generate.String();
//
//            var pipeline = _sut
//                .WithDocumentsFrom(source)
//                .ClassifyWith(classifierName);
//
//            pipeline.Subscribe(t =>
//            {
//                t.HttpDocument.Received(1).Classify(classifierName);
//                Assert.NotNull(t.ClassificationResults);
//            });
//        }

        [Fact]
        public void Then_invokes_the_supplied_Action()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var actionInvoked = false;
            var pipeline = _sut.WithDocumentsFrom(source)
                .Then(d => actionInvoked = true);

            pipeline.Start();

            Assert.True(actionInvoked);
        }

        [Fact]
        public async Task Then_invokes_the_supplied_async_Action()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var completion = new TaskCompletionSource<bool>();
            var pipeline = _sut.WithDocumentsFrom(source)
                .Then(async d => await Task.Run(() => completion.SetResult(true)));

            pipeline.Start();
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

            pipeline.Start();
            var funcWasCalled = await completion.Task;

            Assert.True(funcWasCalled);
        }

        [Fact]
        public void The_document_is_deleted_when_processing_completes()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            _sut
                .WithDocumentsFrom(source)
                .Then(d => d.HttpDocument.Received(1).Delete(Arg.Any<Action>()))
                .Start();
        }

        [Fact]
        public void OnDocumentError_is_run_when_a_document_has_a_processing_error()
        {
            var onDocumentErrorActionRun = false;
            var document = new TestDocument(Generate.Bytes());
            var exception = new Exception("An exception");
            var source = Observable.Repeat(document, 1);

            _sut.WithDocumentsFrom(source)
                .Then(d => throw exception)
                .OnDocumentError(err =>
                {
                    Assert.Same(document, err.Document);
                    Assert.Same(exception, err.Exception);
                    onDocumentErrorActionRun = true;
                })
                .Start();

            Assert.True(onDocumentErrorActionRun);
        }

        [Fact]
        public void OnDocumentError_is_run_when_a_document_has_an_error_during_creation()
        {
            var onDocumentErrorActionRun = false;
            var document = new TestDocument(Generate.Bytes());
            var exception = new Exception("Could not create document");
            var source = Observable.Repeat(document, 1);

            _documentFactory
                .CreateDocument(Arg.Any<Document>())
                .Throws(exception);

            _sut.WithDocumentsFrom(source)
                .OnDocumentError(err =>
                {
                    Assert.Same(document, err.Document);
                    Assert.NotNull(err.Exception);
                    Assert.Null(err.Exception.InnerException);
                    Assert.Equal(err.Exception.Message, exception.Message);
                    onDocumentErrorActionRun = true;
                })
                .Start();

                Assert.True(onDocumentErrorActionRun);
        }

        [Fact]
        public void OnDocumentError_is_run_multiple_times_in_correct_order_if_specified()
        {
            var onDocumentErrorCalledFor = new List<(DocumentError error, int sequence)>();
            var document = new TestDocument(Generate.Bytes());
            var exception = new Exception("Could not create document");
            var source = Observable.Repeat(document, 1);

            _documentFactory
                .CreateDocument(Arg.Any<Document>())
                .Throws(exception);

            _sut.WithDocumentsFrom(source)
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
                .Start();

            Assert.Equal(2, onDocumentErrorCalledFor.Count);
            Assert.Equal(1, onDocumentErrorCalledFor.First().sequence);
            Assert.Equal(2, onDocumentErrorCalledFor.Last().sequence);
        }
    }
}
