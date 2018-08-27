using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Waives.Http.Logging;
using Xunit;
using NSubstitute;

namespace Waives.Http.Tests
{
    public class WaivesClientFacts
    {
        private readonly IHttpRequestSender _requestSender;
        private readonly WaivesClient _sut;
        private readonly byte[] _documentContents;

        public WaivesClientFacts()
        {
            _requestSender = Substitute.For<IHttpRequestSender>();
            _sut = new WaivesClient(new HttpClient(),
                Substitute.For<ILogger>(),
                _requestSender);

            _documentContents = new byte[] { 0, 1, 2 };
        }

        [Fact]
        public async Task CreateDocument_sends_request_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ACreateDocumentResponse());

            using (var stream = new MemoryStream(_documentContents))
            {
                await _sut.CreateDocument(stream);
            }

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri.ToString() == "/documents"));
        }

        [Fact]
        public async Task CreateDocument_sends_the_supplied_stream_as_content()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ACreateDocumentResponse());

            using (var stream = new MemoryStream(_documentContents))
            {
                await _sut.CreateDocument(stream);

                await _requestSender
                    .Received(1)
                    .Send(Arg.Is<HttpRequestMessage>(m =>
                        RequestContentEquals(m, _documentContents)));
            }
        }

        [Fact]
        public async Task CreateDocument_sends_the_supplied_file_as_content()
        {
            const string filePath = "DummyDocument.txt";

            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ACreateDocumentResponse());

            await _sut.CreateDocument(filePath);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    RequestContentEquals(m, File.ReadAllBytes(filePath))));
        }

        [Fact]
        public async Task CreateDocument_returns_a_document_with_correct_properties_set()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ACreateDocumentResponse());

            using (var stream = new MemoryStream(_documentContents))
            {
                var document = await _sut.CreateDocument(stream);

                Assert.Equal("expectedDocumentId", document.Id);
                Assert.Equal(new[]
                    { "document:read", "document:classify", "self" },
                    document.Behaviours.Keys);
            }
        }

        [Fact]
        public async Task GetAllDocuments_sends_request_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AGetAllDocumentsResponse());

            await _sut.GetAllDocuments();

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Get &&
                    m.RequestUri.ToString() == "/documents"));
        }

        [Fact]
        public async Task GetAllDocuments_returns_a_correct_set_of_documents()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AGetAllDocumentsResponse());

            var documents = await _sut.GetAllDocuments();

            var documentsArray = documents.ToArray();
            Assert.Equal("expectedDocumentId1", documentsArray.First().Id);
            Assert.Equal("expectedDocumentId2", documentsArray.Last().Id);
            Assert.Equal(new[] { "document:read", "document:classify", "self" },
                documentsArray.First().Behaviours.Keys);
        }

        [Fact]
        public async Task Login_sends_correct_request()
        {
            const string expectedClientId = "clientid";
            const string expectedClientSecret = "clientsecret";

            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AGetTokenResponse());

            await _sut.Login(expectedClientId, expectedClientSecret);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Post &&
                    IsFormWithClientCredentials(m.Content, expectedClientId, expectedClientSecret)));
        }

        private bool IsFormWithClientCredentials(HttpContent content, string expectedClientId, string expectedClientSecret)
        {
            if (!(content is FormUrlEncodedContent))
            {
                return false;
            }

            var formData = content.ReadAsFormDataAsync().Result;

            Assert.Equal(expectedClientId, formData["client_id"]);
            Assert.Equal(expectedClientSecret, formData["client_secret"]);

            return true;
        }

        private bool RequestContentEquals(HttpRequestMessage request, byte[] expectedContents)
        {
            var actualRequestContents = request.Content.ReadAsByteArrayAsync().Result;

            return actualRequestContents.SequenceEqual(expectedContents);
        }

        private static HttpResponseMessage ACreateDocumentResponse()
        {
            return
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(CreateDocumentResponse)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                    }
                };
        }


        private static HttpResponseMessage AGetAllDocumentsResponse()
        {
            return
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(GetAllDocumentsResponse)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                    }
                };
        }

        private static HttpResponseMessage AGetTokenResponse()
        {
            return
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(GetTokenResponse)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                    },
                };
        }

        private const string GetTokenResponse = @"{
	        ""access_token"": ""token"",
	        ""token_type"": ""Bearer"",
	        ""expires_in"": 86400}";

        private const string CreateDocumentResponse = @"{
            ""id"": ""expectedDocumentId"",
            ""_links"": {
                ""document:read"": {
                    ""href"": ""/documents/LAHV1hoYikqukLpuhiFpAw/reads""
                },
                ""document:classify"": {
                    ""href"": ""/documents/LAHV1hoYikqukLpuhiFpAw/classify/{classifier_name}"",
                    ""templated"": true
                },
                ""self"": {
                    ""href"": ""/documents/LAHV1hoYikqukLpuhiFpAw""
                }
            },
            ""_embedded"": {
                ""files"": [
                {
                    ""id"": ""HEE7UnX680y7yecR-yXsPA"",
                    ""file_type"": ""Image:TIFF"",
                    ""size"": 41203,
                    ""sha256"": ""eeea8dbbf4f0da70bf3dcc25ee0ecf5c6f8a4eae2817fe782a59589cbd4cb9fd""
                }]
            }
        }";

        private const string GetAllDocumentsResponse = @"{
	        ""documents"": [
              {
                ""id"": ""expectedDocumentId1"",
                ""_links"": {
                    ""document:read"": {
                        ""href"": ""/documents/expectedDocumentId1/reads""
                    },
                    ""document:classify"": {
                        ""href"": ""/documents/expectedDocumentId1/classify/{classifier_name}"",
                        ""templated"": true
                    },
                    ""self"": {
                        ""href"": ""/documents/expectedDocumentId1""
                    }
                },
                ""_embedded"": {
                    ""files"": [
                    {
                        ""id"": ""HEE7UnX680y7yecR-yXsPA"",
                        ""file_type"": ""Image:TIFF"",
                        ""size"": 41203,
                        ""sha256"": ""eeea8dbbf4f0da70bf3dcc25ee0ecf5c6f8a4eae2817fe782a59589cbd4cb9fd""

                    }
                    ]
                 }
               },
               {
                 ""id"": ""expectedDocumentId2"",
                 ""_links"": {
                    ""document:read"": {
                        ""href"": ""/documents/expectedDocumentId2/reads""
                    },
                    ""document:classify"": {
                        ""href"": ""/documents/expectedDocumentId2/classify/{classifier_name}"",
                        ""templated"": true
                    },
                    ""self"": {
                        ""href"": ""/documents/expectedDocumentId2""
                    }
                 },
                 ""_embedded"": {
                    ""files"": [
                    {
                        ""id"": ""YY-WZbHuukCOXMalCZ3rBA"",
                        ""file_type"": ""Image:TIFF"",
                        ""size"": 41203,
                        ""sha256"": ""eeea8dbbf4f0da70bf3dcc25ee0ecf5c6f8a4eae2817fe782a59589cbd4cb9fd""
                    }
                    ]
                }
            }
            ]
        }";
    }
}