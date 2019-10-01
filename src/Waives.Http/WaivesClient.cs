using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Waives.Http.RequestHandling;
using Waives.Http.Requests;
using Waives.Http.Responses;

[assembly: InternalsVisibleTo("Waives.Http.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Waives.Pipelines")]
namespace Waives.Http
{
    /// <summary>
    /// The top-level client class for communicating with the Waives platform API.
    /// </summary>
    public sealed class WaivesClient
    {
        internal const string DefaultUrl = "https://api.waives.io";

        private readonly IHttpRequestSender _requestSender;

        /// <summary>
        /// Creates a new instance of <see cref="WaivesClient"/> using the given
        /// API URI and logger. By default, the client is created for the public
        /// Waives platform at https://api.waives.io/, and no logger is used.
        /// <see cref="Login"/> must be called on this client before attempting to
        /// use it for document operations.
        /// </summary>
        /// <param name="apiUri">The URI of the Waives API instance to use. Defaults
        /// to https://api.waives.io/. </param>
        /// <returns>A new <see cref="WaivesClient"/> instance ready for
        /// authentication. <see cref="Login"/> must be called on this client before
        /// attempting to use it for document operations.
        /// </returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public static class Program
        /// {
        ///     public static async Task Main(string[] args)
        ///     {
        ///         var client = WaivesClient.Create().Login("my-client-id", "my-client-secret");
        ///         foreach (var document in await client.GetAllDocuments())
        ///         {
        ///             await Console.WriteLineAsync(document.Id);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static WaivesClient Create(Uri apiUri = null)
        {
            apiUri = apiUri ?? new Uri(DefaultUrl);

            var requestSender = new LoggingRequestSender(
                new TimeoutHandlingRequestSender(
                    new FailedRequestHandlingRequestSender(
                        new ReliableRequestSender(new RequestSender(
                                new HttpClient
                                {
                                    BaseAddress = apiUri
                                })))));


            return new WaivesClient(requestSender);
        }

        internal WaivesClient(IHttpRequestSender requestSender)
        {
            _requestSender = requestSender;
        }

        /// <summary>
        /// Gets or sets a duration on the underlying <see cref="System.Net.Http.HttpClient"/>
        /// to wait until the requests time out. The timeout unit is seconds, and defaults to 120.
        /// </summary>
        /// <seealso cref="System.Net.Http.HttpClient.Timeout"/>
        public int Timeout
        {
            get => _requestSender.Timeout;
            set => _requestSender.Timeout = value;
        }

        /// <summary>
        /// Authenticate your application with the Waives platform.
        /// </summary>
        /// <param name="clientId">The Waives API Client ID to use when authenticating with the service.</param>
        /// <param name="clientSecret">The Waives API Client Secret to use when authenticating with the
        /// service.</param>
        public WaivesClient Login(string clientId, string clientSecret)
        {
            var accessTokenService = new AccessTokenService(
                clientId, clientSecret,
                _requestSender);

            return new WaivesClient(new TokenFetchingRequestSender(accessTokenService, _requestSender));
        }

        /// <summary>
        /// Creates a new document in the Waives platform from the provided <see cref="Stream"/>.
        /// </summary>
        /// <remarks>
        /// The Waives platform implements a limit on the number of documents that may concurrently
        /// exist within your account. It is expected that documents will exist only transiently
        /// within the Waives platform, and must be deleted after all desired operations have been
        /// completed on them. It can be useful to use <see cref="GetAllDocumentsAsync"/> in conjunction
        /// with <see cref="Document.DeleteAsync"/> to ensure you are starting from a clean slate.
        /// </remarks>
        /// <param name="documentSource">The <see cref="Stream"/> source of the document.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>A <see cref="Document"/> client for the given document.</returns>
        /// <seealso cref="Document"/>
        /// <seealso cref="Document.DeleteAsync"/>
        public async Task<Document> CreateDocumentAsync(Stream documentSource, CancellationToken cancellationToken = default)
        {
            if (documentSource == null)
            {
                throw new ArgumentNullException(nameof(documentSource));
            }

            if (!documentSource.CanSeek)
            {
                // This Stream implementation from ASP.NET Core buffers streams larger than the specified threshold to
                // a temporary file, up to the buffer limit. Furthermore, it is a seekable Stream, so we can query the
                // Stream's length
                documentSource = new FileBufferingReadStream(
                    documentSource,
                    1024 * 1024, /* 1MiB */
                    30 * 1000 * 1000, /* 30 MB */
                    Path.GetTempPath);

                await documentSource.ReadAsync(new byte[1], 0, 1, cancellationToken).ConfigureAwait(false);
                documentSource.Seek(0, SeekOrigin.Begin);
            }

            if (documentSource.Length < 1)
            {
                throw new ArgumentException("The provided stream has no content.", nameof(documentSource));
            }

            var request =
                new HttpRequestMessageTemplate(HttpMethod.Post, new Uri("/documents", UriKind.Relative))
                {
                    Content = new StreamContent(documentSource)
                };

            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var document = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);

            return new Document(_requestSender, document.Id, document.Links);
        }

        /// <summary>
        /// Creates a new document in the Waives platform from the provided file path.
        /// </summary>
        /// <remarks>
        /// The Waives platform implements a limit on the number of documents that may concurrently
        /// exist within your account. It is expected that documents will exist only transiently
        /// within the Waives platform, and must be deleted after all desired operations have been
        /// completed on them. It can be useful to use <see cref="GetAllDocumentsAsync"/> in conjunction
        /// with <see cref="Document.DeleteAsync"/> to ensure you are starting from a clean slate.
        /// </remarks>
        /// <param name="path">A path to a file on disk from which the document will be created.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>A <see cref="Document"/> client for the given document.</returns>
        /// <seealso cref="Document"/>
        /// <seealso cref="Document.DeleteAsync"/>
        public async Task<Document> CreateDocumentAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
            }

            return await CreateDocumentAsync(File.OpenRead(path), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new document in the Waives platform from a file available at the specified URI.
        /// </summary>
        /// <remarks>
        /// The Waives platform implements a limit on the number of documents that may concurrently
        /// exist within your account. It is expected that documents will exist only transiently
        /// within the Waives platform, and must be deleted after all desired operations have been
        /// completed on them. It can be useful to use <see cref="GetAllDocumentsAsync"/> in conjunction
        /// with <see cref="Document.DeleteAsync"/> to ensure you are starting from a clean slate.
        /// </remarks>
        /// <param name="uri">The HTTP(S) URI of a file, accessible to Waives, from which the document will be created.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>A <see cref="Document"/> client for the given document.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided <see cref="Uri"/> is null.</exception>
        /// <seealso cref="Document"/>
        /// <seealso cref="Document.DeleteAsync"/>
        public async Task<Document> CreateDocumentAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));

            var request =
                new HttpRequestMessageTemplate(HttpMethod.Post, new Uri($"/documents", UriKind.Relative))
                {
                    Content = new JsonContent(new ImportDocumentRequest
                    {
                        Url = uri.ToString()
                    })

                };

            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            var id = responseContent.Id;
            var behaviours = responseContent.Links;

            return new Document(_requestSender, id, behaviours);
        }

        /// <summary>
        /// Fetch a reference for the given document in the Waives platform.
        /// </summary>
        /// <param name="id">The ID of the document, as returned by <see cref="CreateDocumentAsync(Stream, CancellationToken)"/>.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>A <see cref="Document"/> client for the specified document ID.</returns>
        public async Task<Document> GetDocumentAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            var request = new HttpRequestMessageTemplate(HttpMethod.Get, new Uri($"/documents/{id}", UriKind.Relative));
            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<HalResponse>().ConfigureAwait(false);
            return new Document(_requestSender, responseContent.Id, responseContent.Links);
        }

        /// <summary>
        /// Retrieves all documents created in your account in the Waives platform.
        /// </summary>
        /// <remarks>
        /// The Waives platform implements a limit on the number of documents that may concurrently
        /// exist within your account. It is expected that documents will exist only transiently
        /// within the Waives platform, and must be deleted after all desired operations have been
        /// completed on them. It can be useful to use <see cref="GetAllDocumentsAsync"/> in conjunction
        /// with <see cref="Document.DeleteAsync"/> to ensure you are starting from a clean slate.
        /// </remarks>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>An <see cref="IEnumerable{Document}"/> representing all the documents
        /// created in your account.</returns>
        public async Task<IEnumerable<Document>> GetAllDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative));
            var response = await _requestSender.SendAsync(request, cancellationToken).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsAsync<DocumentCollection>().ConfigureAwait(false);
            return responseContent.Documents.Select(d => new Document(_requestSender, d.Id, d.Links));
        }
    }
}