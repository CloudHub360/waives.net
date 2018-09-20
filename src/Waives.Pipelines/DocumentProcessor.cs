using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class DocumentProcessor<T> : IDocumentProcessor<T>
    {
        private readonly IEnumerable<Func<T, Task<T>>> _docActions;
        private readonly Action<Exception, T> _onDocumentException;
        private readonly TaskScheduler _callbackScheduler;

        public DocumentProcessor(IEnumerable<Func<T, Task<T>>> docActions,
            Action<Exception, T> onDocumentException,
            TaskScheduler callbackScheduler)
        {
            _docActions = docActions ?? throw new ArgumentNullException(nameof(docActions));
            _onDocumentException = onDocumentException ?? throw new ArgumentNullException(nameof(onDocumentException));
            _callbackScheduler = callbackScheduler ?? throw new ArgumentNullException(nameof(callbackScheduler));
        }

        public Task Run(T doc)
        {
            return PerformDocActions(doc)
                .ContinueWith(
                    t =>
                    {
                        _onDocumentException(t.Exception, doc);
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    _callbackScheduler);
        }

        private async Task PerformDocActions(T doc)
        {
            foreach (var docAction in _docActions)
            {
                // mmm, curry.
                doc = await docAction(doc).ConfigureAwait(false);
            }
        }
    }
}