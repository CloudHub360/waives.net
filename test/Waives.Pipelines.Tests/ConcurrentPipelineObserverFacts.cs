using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Waives.Pipelines.Tests
{
    public class ConcurrentPipelineObserverFacts
    {
        private readonly IDocumentProcessor _documentProcessor;
        private readonly TaskCompletionSource<bool> _tcs;
        private readonly Action _onPipelineCompleted;
        private readonly TestDocument _testDocument;
        private readonly IObservable<TestDocument> _source;
        private readonly int _maxConcurrency;
        private readonly Action<Exception> _onPipelineError;

        public ConcurrentPipelineObserverFacts()
        {
            _documentProcessor = Substitute.For<IDocumentProcessor>();
            _tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _onPipelineCompleted = () => { _tcs.SetResult(true); };
            _onPipelineError = e => { _tcs.SetException(e); };
            _testDocument = new TestDocument(Generate.Bytes());
            _maxConcurrency = 100;
            _source = Observable.Repeat(_testDocument, _maxConcurrency);
        }

        [Fact]
        public async Task Call_onError_handler_if_observable_throws()
        {
            var innerException = new Exception();
            var sut = new ConcurrentPipelineObserver(_documentProcessor,
                _onPipelineCompleted,
                _onPipelineError,
                1);

            Observable.Throw<Document>(innerException).Subscribe(sut);

            var pipelineException = await Assert.ThrowsAsync<PipelineException>(async () => await _tcs.Task);
            Assert.Same(innerException, pipelineException.InnerException);
        }

        [Fact]
        public async Task Call_DocumentProcessor_for_each_observed_document()
        {
            var sut = new ConcurrentPipelineObserver(_documentProcessor,
                _onPipelineCompleted,
                _onPipelineError,
                1);

            _source.Subscribe(sut);

            await _tcs.Task;
            await _documentProcessor.Received(_maxConcurrency).RunAsync(_testDocument);
        }

        [Fact]
        public async Task Call_onComplete_handler_once_only()
        {
            var callCount = 0;

            void OnPipelineCompleted()
            {
                callCount++;
                _tcs.TrySetResult(true);
            }

            var sut = new ConcurrentPipelineObserver(_documentProcessor,
                OnPipelineCompleted,
                _onPipelineError,
                _maxConcurrency);

            _source.Subscribe(sut);

            await _tcs.Task;
            Assert.Equal(1, callCount);
        }
    }
}