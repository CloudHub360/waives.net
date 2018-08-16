using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Waives.Reactive.HttpAdapters;

namespace Waives.Reactive
{
    /// <summary>
    /// Configures a new document-processing pipeline.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// using System;
    /// using System.Threading.Tasks;
    /// using Waives.Reactive;
    /// using Waives.Reactive.Extensions.DocumentSources.Filesystem
    ///
    /// namespace Waives.Reactive.Example
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
            var rateLimitedDocument = _rateLimiter.RateLimited(documentSource);

            _pipeline = rateLimitedDocument.SelectMany(
                async d => new WaivesDocument(d, await _documentFactory.CreateDocument(d).ConfigureAwait(false)));

            return this;
        }

        /// <summary>
        /// Classify a document with the specified classifier, optionally only if the specified filter returns True
        /// </summary>
        /// <param name="classifierName"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline ClassifyWith(string classifierName)
        {
            _pipeline = _pipeline.SelectMany(
                async d => await d.Classify(classifierName).ConfigureAwait(false));

            return this;
        }

        /// <summary>
        /// Run an arbitrary transform on each document, doing something with results for example
        /// </summary>
        /// <param name="func"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Func<WaivesDocument, Task<WaivesDocument>> func)
        {
            _pipeline = _pipeline.SelectMany(func);
            return this;
        }

        /// <summary>
        /// Run an arbitrary action on each document, doing something with results for example
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Action<WaivesDocument> action)
        {
            _pipeline = _pipeline.Select(d =>
            {
                action(d);
                return d;
            });

            return this;
        }

        /// <summary>
        /// Run an arbitrary action on each document, doing something with results for example
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Func<WaivesDocument, Task> action)
        {
            _pipeline = _pipeline.SelectMany(async d =>
            {
                await action(d).ConfigureAwait(false);
                return d;
            });

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
    }
}