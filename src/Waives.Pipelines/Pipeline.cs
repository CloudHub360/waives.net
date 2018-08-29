using System;
using System.Collections.Generic;
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
    public class Pipeline : IObservable<Document>
    {
        private readonly IHttpDocumentFactory _documentFactory;
        private readonly int _maxConcurrency;
        private IObservable<Document> _docSource = Observable.Empty<Document>();
        private Action _onPipelineCompleted = () => { };
        private readonly Action<Document, Exception> _onDocumentError;
        private Action<Document, Exception> _userErrorAction = (d, ex) => { };

        private readonly List<Func<WaivesDocument, Task<WaivesDocument>>> _docActions =
            new List<Func<WaivesDocument, Task<WaivesDocument>>>();

        internal Pipeline(IHttpDocumentFactory documentFactory, int maxConcurrency)
        {
            _documentFactory = documentFactory;
            _maxConcurrency = maxConcurrency;
            _onDocumentError = (doc, ex) =>
            {
                Console.WriteLine($"An error occurred during processing of {doc}. " +
                                  $"The error was: {ex.GetType().Name} {ex.Message}");
                _userErrorAction(doc, ex);
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
            _docSource = documentSource;

            return this;
        }

        /// <summary>
        /// Classify a document with the specified classifier, optionally only if the specified filter returns True
        /// </summary>
        /// <param name="classifierName"></param>
        /// <returns>The modified <see cref="Pipeline"/>.</returns>
        public Pipeline ClassifyWith(string classifierName)
        {
            _docActions.Add(async d => await d.Classify(classifierName));
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
                await action(d);
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
            _docActions.Add(async d => await action(d));

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
        public Pipeline OnDocumentError(Action<Document, Exception> action)
        {
            var userAction = action ?? ((doc, ex) => { });

            var previousAction = _userErrorAction;
            _userErrorAction = (doc, ex) =>
            {
                previousAction(doc, ex);
                userAction(doc, ex);
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
            async Task FullPipeline(Document d)
            {
                var waivesDoc = new WaivesDocument(d, await _documentFactory.CreateDocument(d));
                try
                {
                    foreach (var docAction in _docActions)
                    {
                        waivesDoc = await docAction(waivesDoc);
                    }
                }
                finally
                {
                    await waivesDoc.HttpDocument.Delete(() => { });
                }
            }

            var pipelineObserver = new ConcurrentObserver<Document>(FullPipeline,
                _onPipelineCompleted,
                _onDocumentError,
                _maxConcurrency);

            return pipelineObserver.SubscribeTo(_docSource);
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<Document> observer)
        {
            return observer.SubscribeTo(_docSource);
        }
    }
}