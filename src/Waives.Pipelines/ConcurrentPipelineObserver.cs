using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class ConcurrentPipelineObserver : IObserver<Document>, IDisposable
    {
        private readonly IDocumentProcessor _documentProcessor;
        private readonly Action _onPipelineCompleted;
        private readonly Action<Exception> _onError;
        private readonly CancellationToken _cancellationToken;

        private readonly WorkPool _workPool;

        internal ConcurrentPipelineObserver(
            IDocumentProcessor documentProcessor,
            Action onPipelineCompleted,
            Action<Exception> onError,
            int maxConcurrency,
            CancellationToken cancellationToken = default)
        {
            _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
            _onPipelineCompleted = onPipelineCompleted ?? throw new ArgumentNullException(nameof(onPipelineCompleted));
            _onError = onError ?? throw new ArgumentNullException(nameof(onError));
            _cancellationToken = cancellationToken;
            _workPool = new WorkPool(maxConcurrency);
        }

        public void OnCompleted()
        {
             _workPool.WaitAsync(_cancellationToken)
                .ContinueWith(_ => _onPipelineCompleted(), _cancellationToken)
                .ConfigureAwait(false);
        }

        public void OnError(Exception error)
        {
            _onError(new PipelineException(
                $"A fatal error occurred in the processing pipeline: {error.Message}",
                error));
        }

        public void OnNext(Document document)
        {
            _workPool.Post(
                async () => await _documentProcessor.RunAsync(document, _cancellationToken),
                _cancellationToken);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _workPool.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}