using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Waives.Pipelines.HttpAdapters;

namespace Waives.Pipelines
{
    /// <summary>
    /// Configures a new document-processing pipeline.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// using System;
    /// using System.Threading.Tasks;
    /// using Waives.Pipelines;
    /// using Waives.Pipelines.Extensions.DocumentSources.FileSystem
    ///
    /// namespace Waives.Pipelines.Example
    /// {
    ///     public class Program
    ///     {
    ///         public static void Main(string[] args)
    ///         {
    ///             Task.Run(() => MainAsync(args)).Wait();
    ///         }
    ///
    ///         public static async Task MainAsync(string[] args)
    ///         {
    ///             await WaivesApi.Login("clientId", "clientSecret");
    ///             var pipeline = WaivesApi.CreatePipeline();
    ///
    ///             pipeline
    ///                 .WithDocumentsFrom(FileSystemSource.Create(@"C:\temp\inbox"))
    ///                 .ClassifyWith("mortgages")
    ///                 .Then(d => Console.WriteLine(d.ClassificationResults.DocumentType))
    ///                 .OnPipelineCompeleted(_ => Console.WriteLine("Processing complete!"));
    ///
    ///             try
    ///             {
    ///                 pipeline.Start();
    ///             }
    ///             catch (WaivesException ex)
    ///             {
    ///                 Console.WriteLine($"Pipeline processing failed: {ex.InnerException.Message}");
    ///             }
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    public class Pipeline : IObservable<WaivesDocument>
    {
        private readonly IHttpDocumentFactory _documentFactory;
        private IObservable<WaivesDocument> _pipeline = Observable.Empty<WaivesDocument>();
        private Action _onPipelineCompleted = () => { };
        private Action<DocumentError> _onDocumentError = (pe) => { };
        private readonly IRateLimiter _rateLimiter;

        internal Pipeline(IHttpDocumentFactory documentFactory, IRateLimiter rateLimiter)
        {
            _documentFactory = documentFactory;
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        }

        /// <summary>
        /// Add the source of documents to the pipeline. This represents the start
        /// of the pipeline
        /// </summary>
        /// <param name="documentSource"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline WithDocumentsFrom(IObservable<Document> documentSource)
        {
            var rateLimitedDocuments = _rateLimiter.RateLimited(documentSource);

            _pipeline = rateLimitedDocuments.Process(async d =>
            {
                return new WaivesDocument(d, await _documentFactory.CreateDocument(d).ConfigureAwait(false));
            }, OnDocumentCreationError);

            return this;
        }

        /// <summary>
        /// Classify a document with the specified classifier, optionally only if the specified filter returns True
        /// </summary>
        /// <param name="classifierName"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline ClassifyWith(string classifierName)
        {
            _pipeline = _pipeline.Process(async d => await d.Classify(classifierName).ConfigureAwait(false), OnProcessingError);

            return this;
        }

        /// <summary>
        /// Run an arbitrary transform on each document, doing something with results for example
        /// </summary>
        /// <param name="func"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Func<WaivesDocument, Task<WaivesDocument>> func)
        {
            _pipeline = _pipeline.Process(func, OnProcessingError);

            return this;
        }

        /// <summary>
        /// Run an arbitrary action on each document, doing something with results for example
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Action<WaivesDocument> action)
        {
            _pipeline = _pipeline.Process(async d =>
            {
                action(d);
                return d;
            }, OnProcessingError);

            return this;
        }

        /// <summary>
        /// Run an arbitrary action on each document, doing something with results for example
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Func<WaivesDocument, Task> action)
        {
            _pipeline = _pipeline.Process(async d =>
            {
                await action(d).ConfigureAwait(false);
                return d;
            }, OnProcessingError);

            return this;
        }

        /// <summary>
        /// Run an arbitrary action when all documents have been processed.
        /// </summary>
        /// <param name="action">The action to execute when the pipeline completes.</param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline OnPipelineCompleted(Action action)
        {
            _onPipelineCompleted = action ?? (() => {});
            return this;
        }

        /// <summary>
        /// Run an arbitrary action when a document has an error during processing.
        /// </summary>
        /// <param name="action">The action to execute when document has an error.</param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline OnDocumentError(Action<DocumentError> action)
        {
            _onDocumentError = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        /// <summary>
        /// Start processing the documents in the pipeline.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> subscription object. If you wish to terminate
        /// the document processing pipeline manually, invoke <see cref="IDisposable.Dispose"/> on
        /// this object.</returns>
        public IDisposable Start()
        {
            _pipeline = _pipeline.SelectMany(async d =>
            {
                await d.HttpDocument.Delete(() =>
                {
                    _rateLimiter.MakeDocumentSlotAvailable();
                }).ConfigureAwait(false);

                return d;
            });

            var pipelineObserver = new PipelineObserver(_onPipelineCompleted);
            return pipelineObserver.SubscribeTo(_pipeline);
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<WaivesDocument> observer)
        {
            return observer.SubscribeTo(_pipeline);
        }

        private async Task OnDocumentCreationError(ProcessingError<Document> error)
        {
            Console.WriteLine($"An error occurred during creation {error.Document.SourceId}. " +
                                $"The error was: {error.Exception.GetType().Name} {error.Exception.Message}");

            _rateLimiter.MakeDocumentSlotAvailable();

            _onDocumentError(new DocumentError(error.Document, error.Exception));
        }

        private async Task OnProcessingError(ProcessingError<WaivesDocument> error)
        {
            Console.WriteLine($"An error occurred during processing of {error.Document.Source.SourceId}. " +
                              $"The error was: {error.Exception.GetType().Name} {error.Exception.Message}");

            await DeleteDocumentAndNotifyRateLimiter(error.Document).ConfigureAwait(false);

            _onDocumentError(new DocumentError(error.Document.Source, error.Exception));
        }

        private async Task DeleteDocumentAndNotifyRateLimiter(WaivesDocument document)
        {
            //TODO: Work out error handling and failure behaviour here
            await document.HttpDocument.Delete(() =>
            {
                _rateLimiter.MakeDocumentSlotAvailable();
            }).ConfigureAwait(false);
        }
    }
}