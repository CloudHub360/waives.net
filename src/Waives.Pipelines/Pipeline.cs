using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
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
        private readonly int _maxConcurrency;
        private IObservable<WaivesDocument> _docSource = Observable.Empty<WaivesDocument>();
        private Action _onPipelineCompleted = () => { };
        private readonly Action<DocumentError> _onDocumentError;
        private Action<DocumentError> _userErrorAction = err => { };
        private readonly ILogger _logger;

        private readonly List<Func<WaivesDocument, Task<WaivesDocument>>> _docActions =
            new List<Func<WaivesDocument, Task<WaivesDocument>>>();

        internal Pipeline(IHttpDocumentFactory documentFactory, ILogger logger, int maxConcurrency)
        {
            _documentFactory = documentFactory;
            _maxConcurrency = maxConcurrency;
            _logger = logger ?? new NoopLogger();

            _onDocumentError = err =>
            {
                _logger.Log(LogLevel.Error,
                    $"An error occurred during processing of document '{err.Document.SourceId}'. " +
                    $"'{err.Exception.GetType().Name}' '{err.Exception.Message}'");

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
            _docSource = documentSource.SelectMany(
                async d =>
                {
                    _logger.Log(LogLevel.Info, $"Started processing '{d.SourceId}'");

                    return new WaivesDocument(d,
                        await _documentFactory.CreateDocument(d).ConfigureAwait(false));
                });

            return this;
        }

        /// <summary>
        /// Classify a document with the specified classifier, optionally only if the specified filter returns True
        /// </summary>
        /// <param name="classifierName"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline ClassifyWith(string classifierName)
        {
            _docActions.Add(async d =>
            {
                var document = await d.Classify(classifierName)
                    .ConfigureAwait(false);

                _logger.Log(LogLevel.Info, $"Classified document '{d.Source}'");

                return document;
            });
            return this;
        }

        /// <summary>
        /// Extract data from documents using the specified extractor. The extractor must have
        /// been created previously in your Waives account in order to be used here.
        /// </summary>
        /// <param name="extractorName">The name of the extractor to use.</param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline ExtractWith(string extractorName)
        {
            _docActions.Add(async d =>
            {
                var document = await d.Extract(extractorName).ConfigureAwait(false);
                _logger.Log(LogLevel.Info, $"Extracted data from document '{d.Source}");
                return document;
            });

            return this;
        }

        /// <summary>
        /// Run an arbitrary action on each document, doing something with results for example
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Action<WaivesDocument> action)
        {
            _docActions.Add(document =>
            {
                action(document);
                return Task.FromResult(document);
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
            _docActions.Add(async d =>
            {
                await action(d).ConfigureAwait(false);
                return d;
            });

            return this;
        }

        /// <summary>
        /// Run an arbitrary action on each document, doing something with results for example
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline Then(Func<WaivesDocument, Task<WaivesDocument>> action)
        {
            _docActions.Add(async d => await action(d).ConfigureAwait(false));

            return this;
        }

        /// <summary>
        /// Run an arbitrary action when all documents have been processed.
        /// </summary>
        /// <param name="action">The action to execute when the pipeline completes.</param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline OnPipelineCompleted(Action action)
        {
            _onPipelineCompleted = action ?? (() => { });
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
            _docActions.Add(async d =>
            {
                await d.HttpDocument.Delete().ConfigureAwait(false);

                _logger.Log(LogLevel.Info, $"Completed processing '{d.Source.SourceId}' and deleted Waives document");

                return d;
            });

            _logger.Log(LogLevel.Info, "Pipeline started");
            var callbackScheduler = TaskScheduler.Current;

            var documentProcessor = new DocumentProcessor<WaivesDocument>(
                _docActions,
                (exception, document) =>
                {
                    _onDocumentError(new DocumentError(document.Source, exception));
                },
                callbackScheduler);

            var pipelineObserver = new ConcurrentPipelineObserver<WaivesDocument>(
                documentProcessor,
                _onPipelineCompleted,
                _maxConcurrency,
                callbackScheduler);

            return pipelineObserver.SubscribeTo(_docSource);
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<WaivesDocument> observer)
        {
            return observer.SubscribeTo(_docSource);
        }
    }
}