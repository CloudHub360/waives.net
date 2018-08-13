using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

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
    public class PipelineBuilder
    {
        private IObservable<WaivesDocument> _pipeline = Observable.Empty<WaivesDocument>();

        /// <summary>
        /// Add the source of documents to the pipeline. This represents the start
        /// of the pipeline
        /// </summary>
        /// <param name="documentSource"></param>
        /// <returns>The modified <see cref="PipelineBuilder"/>.</returns>
        public PipelineBuilder WithDocumentsFrom(IObservable<Document> documentSource)
        {
            _pipeline = documentSource.SelectMany(async d =>
            {
                using (var documentStream = await d.OpenStream().ConfigureAwait(false))
                {
                    return new WaivesDocument(d,
                        await WaivesApi.ApiClient.CreateDocument(documentStream).ConfigureAwait(false));
                }
            });

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
            return Disposable.Empty;
        }
    }
}