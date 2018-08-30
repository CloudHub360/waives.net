using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waives.Http;
using Waives.Http.Logging;
using Waives.Pipelines.HttpAdapters;

[assembly: InternalsVisibleTo("Waives.Pipelines.Tests")]
namespace Waives.Pipelines
{
    public static class WaivesApi
    {
        internal static WaivesClient ApiClient { get; private set; }

        /// <summary>
        /// Authenticate against the Waives API.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values for the client ID and secret parameters can be obtained from the Waives
        /// Dashboard, at https://dashboard.waives.io/. These should be treated like a username-
        /// password pair, where the client ID is equivalent to a username, and the client
        /// secret is equivalent to a password. Your client secret MUST be kept secret, and your
        /// client ID SHOULD be kept secret.
        /// </para>
        /// <para>
        /// By default, this library will communicate with the public hosted Waives API at
        /// https://api.waives.io/.  You only need to change this if you are hosting your own
        /// copy of the Waives API.
        /// </para>
        /// </remarks>
        /// <param name="clientId">The ID of the client used to authenticate with the Waives
        /// API.</param>
        /// <param name="clientSecret">The secret of the client used to authentication with the
        /// Waives API.</param>
        /// <param name="apiUri">The instance of the Waives API you wish to use. Defaults to
        /// https://api.waives.io/, the hosted Waives API.</param>
        /// <returns>A new <see cref="WaivesClient"/> instance, with which you can directly
        /// call the Waives API. This is provided for advanced use cases.</returns>
        public static async Task<WaivesClient> Login(string clientId, string clientSecret, Uri apiUri = null)
        {
            apiUri = apiUri ?? new Uri(WaivesClient.DefaultUrl);

            ApiClient = new WaivesClient(apiUri);
            await ApiClient.Login(clientId, clientSecret).ConfigureAwait(false);

            return ApiClient;
        }

        /// <summary>
        /// Configure a new document-processing pipeline.
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
        /// <param name="deleteExistingDocuments">If set to <c>true</c>, it will (immediately) delete all documents
        /// in existence in the Waives account; if set to <c>false</c>, no such clean up will be completed.
        /// Defaults to <c>true</c>.</param>
        /// <param name="logger">A logger that will receive log messages from the pipeline and the underlying <see cref="WaivesClient"/></param>
        /// <param name="maxConcurrency">The maximum number of documents to process concurrently.</param>
        /// <returns>A new <see cref="Pipeline"/> instance with which you can
        /// configure your document processing pipeline.</returns>
        public static Pipeline CreatePipeline(bool deleteExistingDocuments = true, ILogger logger = null, int maxConcurrency = RateLimiter.DefaultMaximumConcurrentDocuments)
        {
            ApiClient.Logger = logger ?? new NoopLogger();

            var documentFactory = Task.Run(() => HttpDocumentFactory.Create(ApiClient, deleteExistingDocuments)).Result;
            return new Pipeline(
                documentFactory,
                new RateLimiter(null, maxConcurrency),
                ApiClient.Logger);
        }
    }
}