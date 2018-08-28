﻿using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Waives.Http.Logging;
using Xunit;
using NSubstitute;
using Waives.Http.Responses;

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
        public async Task CreateDocument_sends_a_request_to_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.CreateDocument());

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
                .Returns(Responses.CreateDocument());

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
                .Returns(Responses.CreateDocument());

            _requestSender
                .Send(Arg.Is<HttpRequestMessage>(m => m.RequestUri.ToString().Contains("/classify")))
                .Returns(Responses.Classify());

            await _sut.CreateDocument(filePath);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    RequestContentEquals(m, File.ReadAllBytes(filePath))));
        }

        [Fact]
        public async Task CreateDocument_returns_a_document_that_can_be_used()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.CreateDocument());

            using (var stream = new MemoryStream(_documentContents))
            {
                var document = await _sut.CreateDocument(stream);

                Assert.Equal("expectedDocumentId", document.Id);

                _requestSender
                    .Send(Arg.Any<HttpRequestMessage>())
                    .Returns(Responses.Classify());

                await document.Classify("classifier");

                _requestSender
                    .Send(Arg.Any<HttpRequestMessage>())
                    .Returns(Responses.Success());

                await document.Delete();
            }
        }

        [Fact]
        public async Task CreateDocument_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.ErrorWithMessage());

            using (var stream = new MemoryStream(_documentContents))
            {
                var exception = await Assert.ThrowsAsync<WaivesApiException>(() =>
                    _sut.CreateDocument(stream));

                Assert.Equal(Responses.ErrorMessage, exception.Message);
            }
        }

        [Fact]
        public async Task GetAllDocuments_sends_a_request_to_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.GetAllDocuments());

            await _sut.GetAllDocuments();

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Get &&
                    m.RequestUri.ToString() == "/documents"));
        }

        [Fact]
        public async Task GetAllDocuments_returns_one_document_for_each_returned_by_the_API()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.GetAllDocuments());

            var documents = await _sut.GetAllDocuments();

            var documentsArray = documents.ToArray();
            Assert.Equal("expectedDocumentId1", documentsArray.First().Id);
            Assert.Equal("expectedDocumentId2", documentsArray.Last().Id);
        }

        [Fact]
        public async Task GetAllDocuments_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.ErrorWithMessage());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.GetAllDocuments());

            Assert.Equal(Responses.ErrorMessage, exception.Message);
        }

        [Fact]
        public async Task Login_sends_a_request_with_the_specified_credentials()
        {
            const string expectedClientId = "clientid";
            const string expectedClientSecret = "clientsecret";

            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.GetToken());

            await _sut.Login(expectedClientId, expectedClientSecret);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Post &&
                    IsFormWithClientCredentials(m.Content, expectedClientId, expectedClientSecret)));
        }

        [Fact]
        public async Task Login_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.ErrorWithMessage());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Login("clientid", "clientsecret"));

            Assert.Equal(Responses.ErrorMessage, exception.Message);
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
    }
}