using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waives.Http;
using Waives.Pipelines.HttpAdapters;

[assembly: InternalsVisibleTo("Waives.Pipelines.Tests")]
namespace Waives.Pipelines
{
    public static class WaivesApi
    {
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
        ///             var pipeline = WaivesApi.CreatePipeline(new WaivesOptions
        ///             {
        ///                 ClientId = "clientId",
        ///                 ClientSecret = "clientSecret"
        ///             });
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
        /// <param name="options"></param>
        /// <returns>A new <see cref="Pipeline"/> instance with which you can
        /// configure your document processing pipeline.</returns>
        public static async Task<Pipeline> CreatePipeline(WaivesOptions options)
        {
            var waivesClient = await CreateAuthenticatedWaivesClient(options).ConfigureAwait(false);

            var documentFactory = await HttpDocumentFactory.Create(
                waivesClient,
                options.Logger,
                options.DeleteExistingDocuments).ConfigureAwait(false);

            return new Pipeline(
                documentFactory,
                options.Logger,
                options.MaxConcurrency);
        }

        private static async Task<WaivesClient> CreateAuthenticatedWaivesClient(WaivesOptions options)
        {
            var client = WaivesClient.Create(options.ApiUrl, options.Logger);
            await client.Login(options.ClientId, options.ClientSecret).ConfigureAwait(false);
            return client;
        }
    }
}