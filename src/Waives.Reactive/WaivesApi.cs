using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waives.Http;

[assembly: InternalsVisibleTo("Waives.Reactive.Tests")]
namespace Waives.Reactive
{
    public static class WaivesApi
    {
        internal static WaivesClient ApiClient { get; private set; } = new WaivesClient();

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
        public static async Task<WaivesClient> Login(
            string clientId, string clientSecret,
            string apiUri = "https://api.waives.io/")
        {
            if (string.IsNullOrWhiteSpace(apiUri))
            {
                throw new ArgumentNullException(nameof(apiUri));
            }

            return await Login(clientId, clientSecret, new Uri(apiUri)).ConfigureAwait(false);
        }

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
        public static async Task<WaivesClient> Login(string clientId, string clientSecret, Uri apiUri)
        {
            ApiClient = new WaivesClient(new HttpClient { BaseAddress = apiUri });
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
        /// <returns>A new <see cref="Pipeline"/> instance with which you can
        /// configure your document processing pipeline.</returns>
        public static Pipeline CreatePipeline()
        {
            return new Pipeline(ApiClient);
        }
    }
}