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
            var results = documents.Select(d =>
            {
                try
                {
                    // TODO: Try again to make this lambda async and not need .Result
                    var waivesDocument = processAction(d).Result;
                    return new ProcessingResult(waivesDocument, true);
                }
                catch (Exception e)
                {
                    errorAction(new ProcessingError<WaivesDocument>(d, e));

                    return new ProcessingResult(d, false);
                }

            }).Where(r => r.ProcessedSuccessfully);

            return results.Select(r => r.Document);
        }

        internal static IObservable<WaivesDocument> Process(this IObservable<Document> documents,
            Func<Document, Task<WaivesDocument>> processAction, Func<ProcessingError<Document>, Task> errorAction)
        {
            var results = documents.Select(d =>
            {
                try
                {
                    // TODO: Try again to make this lambda async and not need .Result
                    var document = processAction(d).Result;
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