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
            Action<DocumentError> errorAction)
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
                    var documentError = await OnProcessingError(
                        new ProcessingError<TDocument>(d, e)).ConfigureAwait(false);

                    errorAction(documentError);

                    return new ProcessingResult(null, false);
                }
            }).Where(r => r.ProcessedSuccessfully);

            return results.Select(r => r.Document);
        }

        private static async Task<DocumentError> OnProcessingError<TDocument>(ProcessingError<TDocument> error)
        {
            // Try coercing the document in error to a Waives Document. If it is, it
            // has been created in the platform and must be deleted before proceeding.
            var waivesDocument = error.Document as WaivesDocument;
            if (waivesDocument != null)
            {
                await waivesDocument.Delete(() => { }).ConfigureAwait(false);
                return new DocumentError(waivesDocument.Source, error.Exception);
            }

            // If it's not a WaivesDocument, it must be a source document and can be
            // safely cast to Document for use in the DocumentError.
            return new DocumentError(error.Document as Document, error.Exception);
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