using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal static class ProcessExtension
    {
        internal static IObservable<WaivesDocument> Process<TDocument>(
            this IObservable<TDocument> documents,
            Func<TDocument, Task<WaivesDocument>> processAction,
            Func<ProcessingError<TDocument>, Task> errorAction)
        {
            var results = documents.SelectMany(async d =>
            {
                try
                {
                    var waivesDocument = await processAction(d).ConfigureAwait(false);
                    return new ProcessingResult(waivesDocument, true);
                }
                catch (Exception e)
                {
                    await errorAction(new ProcessingError<TDocument>(d, e)).ConfigureAwait(false);
                    return new ProcessingResult(d as WaivesDocument, false);
                }
            }).Where(r => r.ProcessedSuccessfully);

            return results.Select(r => r.Document);
        }

        private class ProcessingResult
        {
            public WaivesDocument Document { get; }
            public bool ProcessedSuccessfully { get; }

            public ProcessingResult(WaivesDocument document, bool processedSuccessfully)
            {
                Document = document;
                ProcessedSuccessfully = processedSuccessfully;
            }
        }
    }
}