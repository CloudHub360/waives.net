using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Waives.Http.Responses;
using Waives.Pipelines;
using Waives.Pipelines.HttpAdapters;
using Xunit;

namespace Waives.Reactive.Tests
{
    public class PipelineFacts
    {
        private readonly IHttpDocumentFactory _documentFactory = Substitute.For<IHttpDocumentFactory>();
        private readonly Pipeline _sut;
        private readonly IRateLimiter _rateLimiter = Substitute.For<IRateLimiter>();

        public PipelineFacts()
        {
            var httpDocument = Substitute.For<IHttpDocument>();
            httpDocument.Classify(Arg.Any<string>()).Returns(new ClassificationResult());
            _documentFactory
                .CreateDocument(Arg.Any<Document>())
                .Returns(httpDocument);

            _sut = new Pipeline(_documentFactory, _rateLimiter);
        }

        [Fact]
        public void OnPipelineCompleted_is_run_at_the_end_of_a_successful_pipeline()
        {
            var pipelineCompleted = false;
            _sut.OnPipelineCompleted(() => pipelineCompleted = true);

            _sut.Start();

            Assert.True(pipelineCompleted);
        }

        [Fact]
        public void WithDocumentsFrom_passes_the_document_source_to_the_ratelimiter()
        {
            var rateLimiter = Substitute.For<IRateLimiter>();
            var sut = new Pipeline(_documentFactory, rateLimiter);
            var source = Observable.Repeat<Document>(new TestDocument(Generate.Bytes()), 3);
            sut.WithDocumentsFrom(source);

            rateLimiter
                .Received(1)
                .RateLimited(Arg.Is<IObservable<Document>>(ds =>
                    ReferenceEquals(ds, source)));
        }

        [Fact]
        public void WithDocumentsFrom_projects_the_rate_limited_documents_into_the_pipeline()
        {
            var expectedDocuments = Enumerable.Repeat<Document>(new TestDocument(Generate.Bytes()), 5).ToObservable();

            _rateLimiter
                .RateLimited(Arg.Any<IObservable<Document>>())
                .Returns(expectedDocuments);

            var pipeline = _sut.WithDocumentsFrom(expectedDocuments);

            Assert.Equal(
                expectedDocuments.ToEnumerable(),
                pipeline.Select(d => d.Source).ToEnumerable());
        }

        [Fact]
        public void WithDocumentsFrom_creates_each_document_with_Waives()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);

            var pipeline = _sut.WithDocumentsFrom(source);

            pipeline.Subscribe(t =>
            {
                _documentFactory.Received(1).CreateDocument(t.Source);
            });
        }

        [Fact]
        public void ClassifyWith_classifies_each_document_with_Waives()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);
            var classifierName = Generate.String();
            var pipeline = _sut
                .WithDocumentsFrom(source)
                .ClassifyWith(classifierName);

            pipeline.Subscribe(t =>
            {
                t.HttpDocument.Received(1).Classify(classifierName);
                Assert.NotNull(t.ClassificationResults);
            });
        }

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
        public void A_slot_is_freed_in_the_rate_limiter_when_a_document_has_finishes_its_processing()
        {
            var source = Observable.Repeat(new TestDocument(Generate.Bytes()), 1);

            _sut.WithDocumentsFrom(source)
                .Then(d => _rateLimiter.Received(1).MakeDocumentSlotAvailable())
                .Start();
        }
    }
}
