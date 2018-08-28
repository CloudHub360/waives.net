using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Waives.Http.Logging;
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
        private Action _onPipelineCompleted;
        private readonly Action<DocumentError> _onDocumentError;
        private Action<DocumentError> _userErrorAction = err => { };
        private readonly IRateLimiter _rateLimiter;
        private readonly ILogger _logger;

        internal Pipeline(IHttpDocumentFactory documentFactory, IRateLimiter rateLimiter) : this(documentFactory, rateLimiter, Loggers.NoopLogger)
        { }

        internal Pipeline(IHttpDocumentFactory documentFactory, IRateLimiter rateLimiter, ILogger logger)
        {
            _documentFactory = documentFactory;
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _onDocumentError = err =>
            {
                _logger.Log(LogLevel.Error,
                    $"An error occurred during processing of document '{err.Document.SourceId}'. " +
                    $"The error was: '{err.Exception.GetType().Name}' '{err.Exception.Message}'");

                _rateLimiter.MakeDocumentSlotAvailable();
                _userErrorAction(err);
            };

            _onPipelineCompleted = () =>
            {
                _logger.Log(LogLevel.Info, "Pipeline complete");
            };
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
                    _logger.Log(LogLevel.Info, $"Started processing '{d.SourceId}'");
                    return new WaivesDocument(d, await _documentFactory.CreateDocument(d).ConfigureAwait(false));
                },
                _onDocumentError);

            return this;
        }

        /// <summary>
        /// Classify a document with the specified classifier, optionally only if the specified filter returns True
        /// </summary>
        /// <param name="classifierName"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline ClassifyWith(string classifierName)
        {
            _pipeline = _pipeline.Process(async d =>
            {
                var document = await d.Classify(classifierName).ConfigureAwait(false);
                _logger.Log(LogLevel.Info, $"Classified document '{d.Source}'");
                return document;
            }, _onDocumentError);

            return this;
        }

        /// <summary>
        /// Run an arbitrary transform on each document, doing something with results for example
        /// </summary>
        /// <param name="func"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Func<WaivesDocument, Task<WaivesDocument>> func)
        {
            _pipeline = _pipeline.Process(func, _onDocumentError);

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
            }, _onDocumentError);

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
            }, _onDocumentError);

            return this;
        }

        /// <summary>
        /// Run an arbitrary action when all documents have been processed.
        /// </summary>
        /// <param name="action">The action to execute when the pipeline completes.</param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline OnPipelineCompleted(Action action)
        {
            var userAction = action ?? (() => { });

            var previousAction = _onPipelineCompleted;
            _onPipelineCompleted = () =>
            {
                previousAction();
                userAction();
            };

            return this;
        }

        /// <summary>
        /// Run an arbitrary action when a document has an error during processing.
        /// </summary>
        /// <param name="action">The action to execute when document has an error.</param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline OnDocumentError(Action<DocumentError> action)
        {
            var userAction = action ?? (err => { });

            var previousAction = _userErrorAction;
            _userErrorAction = err =>
            {
                previousAction(err);
                userAction(err);
            };

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
                    _logger.Log(LogLevel.Info, $"Completed processing '{d.Source.SourceId}' and deleted Waives document");
                }).ConfigureAwait(false);

                return d;
            });

            _logger.Log(LogLevel.Info, "Pipeline started");
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