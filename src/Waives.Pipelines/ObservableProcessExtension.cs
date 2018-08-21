using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal static class ProcessExtension
    {
        internal static IObservable<WaivesDocument> Process(this IObservable<WaivesDocument> documents,
            Func<WaivesDocument, Task<WaivesDocument>> processAction, Func<ProcessingError<WaivesDocument>, Task> errorAction)
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
                    await errorAction(new ProcessingError<WaivesDocument>(d, e)).ConfigureAwait(false);

                    return new ProcessingResult(d, false);
                }
            }).Where(r => r.ProcessedSuccessfully);

            return results.Select(r => r.Document);
        }

        internal static IObservable<WaivesDocument> Process(this IObservable<Document> documents,
            Func<Document, Task<WaivesDocument>> processAction, Action<ProcessingError<Document>> errorAction)
        {
            var results = documents.SelectMany(async d =>
            {
                try
                {
                    var document = await processAction(d).ConfigureAwait(false);
                    return new ProcessingResult(document, true);
                }
                catch (Exception e)
                {
                    errorAction(new ProcessingError<Document>(d, e));

                    return new ProcessingResult(null, false);
                }
            }).Where(r => r.ProcessedSuccessfully);

            return results.Select(r => r.Document);
        }
    }
}