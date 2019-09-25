﻿using System;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class ConcurrentPipelineObserver : IObserver<Document>, IDisposable
    {
        private readonly IDocumentProcessor _documentProcessor;
        private readonly Action _onPipelineCompleted;
        private bool _sourceComplete;

        private readonly WorkPool _workPool;

        internal ConcurrentPipelineObserver(
            IDocumentProcessor documentProcessor,
            Action onPipelineCompleted,
            int maxConcurrency)
        {
            _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
            _onPipelineCompleted = onPipelineCompleted ?? throw new ArgumentNullException(nameof(onPipelineCompleted));
            _workPool = new WorkPool(maxConcurrency);
        }

        public void OnCompleted()
        {
            _sourceComplete = true;
            _ = TryCompletionHandler();
        }

        public void OnError(Exception error)
        {
            throw new PipelineException(
                $"A fatal error occurred in the processing pipeline: {error.Message}",
                error);
        }

        public void OnNext(Document doc)
        {
            _workPool.Post(async () =>
            {
                try
                {
                    await _documentProcessor.Run(doc);
                }
                finally
                {
                    await TryCompletionHandler();
                }
            });
        }

        private async Task TryCompletionHandler()
        {
            if (_sourceComplete)
            {
                await _workPool.WaitAsync();
                _onPipelineCompleted();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _workPool?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}