using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Waives.Http.RequestHandling;
using Waives.Http.Tests.RequestHandling;
using Xunit;

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
            _sut = new WaivesClient(_requestSender);

            _documentContents = new byte[] { 0, 1, 2 };
        }

        [Fact]
        public async Task CreateDocument_sends_a_request_to_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.CreateDocument(ci.Arg<HttpRequestMessageTemplate>()));

            using (var stream = new MemoryStream(_documentContents))
            {
                await _sut.CreateDocument(stream);
            }

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri.ToString() == "/documents"));
        }

        [Fact]
        public async Task CreateDocument_sends_the_supplied_stream_as_content()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.CreateDocument(ci.Arg<HttpRequestMessageTemplate>()));

            using (var stream = new MemoryStream(_documentContents))
            {
                await _sut.CreateDocument(stream);

                await _requestSender
                    .Received(1)
                    .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                        RequestContentEquals(m, _documentContents)));
            }
        }

        [Fact]
        public async Task CreateDocument_sends_the_supplied_file_as_content()
        {
            const string filePath = "DummyDocument.txt";

            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.CreateDocument(ci.Arg<HttpRequestMessageTemplate>()));

            _requestSender
                .Send(Arg.Is<HttpRequestMessageTemplate>(m => m.RequestUri.ToString().Contains("/classify")))
                .Returns(ci => Response.Classify(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.CreateDocument(filePath);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    RequestContentEquals(m, File.ReadAllBytes(filePath))));
        }

        [Fact]
        public async Task CreateDocument_returns_a_document_that_can_be_used()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.CreateDocument(ci.Arg<HttpRequestMessageTemplate>()));

            using (var stream = new MemoryStream(_documentContents))
            {
                var document = await _sut.CreateDocument(stream);

                Assert.Equal("expectedDocumentId", document.Id);

                _requestSender
                    .Send(Arg.Any<HttpRequestMessageTemplate>())
                    .Returns(ci => Response.Classify(ci.Arg<HttpRequestMessageTemplate>()));

                await document.Classify("classifier");

                _requestSender
                    .Send(Arg.Any<HttpRequestMessageTemplate>())
                    .Returns(ci => Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

                await document.Delete();
            }
        }

        [Fact]
        public async Task CreateDocument_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(Response.ErrorMessage));

            using (var stream = new MemoryStream(_documentContents))
            {
                var exception = await Assert.ThrowsAsync<WaivesApiException>(() =>
                    _sut.CreateDocument(stream));

                Assert.Equal(Response.ErrorMessage, exception.Message);
            }
        }

        [Fact]
        public async Task GetAllDocuments_sends_a_request_to_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.GetAllDocuments(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.GetAllDocuments();

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Get &&
                    m.RequestUri.ToString() == "/documents"));
        }

        [Fact]
        public async Task GetAllDocuments_returns_one_document_for_each_returned_by_the_API()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.GetAllDocuments(ci.Arg<HttpRequestMessageTemplate>()));

            var documents = await _sut.GetAllDocuments();

            var documentsArray = documents.ToArray();
            Assert.Equal("expectedDocumentId1", documentsArray.First().Id);
            Assert.Equal("expectedDocumentId2", documentsArray.Last().Id);
        }

        [Fact]
        public async Task GetAllDocuments_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(Response.ErrorMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.GetAllDocuments());

            Assert.Equal(Response.ErrorMessage, exception.Message);
        }

        [Fact]
        public async Task Login_sends_a_request_with_the_specified_credentials()
        {
            const string expectedClientId = "clientid";
            const string expectedClientSecret = "clientsecret";

            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.GetToken(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Login(expectedClientId, expectedClientSecret);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Post &&
                    IsFormWithClientCredentials(m.Content, expectedClientId, expectedClientSecret)));
        }

        [Fact]
        public async Task Login_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(Response.ErrorMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Login("clientid", "clientsecret"));

            Assert.Equal(Response.ErrorMessage, exception.Message);
        }

        private static bool IsFormWithClientCredentials(HttpContent content, string expectedClientId, string expectedClientSecret)
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

        private static bool RequestContentEquals(HttpRequestMessageTemplate request, byte[] expectedContents)
        {
            var actualRequestContents = request.Content.ReadAsByteArrayAsync().Result;

            return actualRequestContents.SequenceEqual(expectedContents);
        }
    }
}