using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NSubstitute;
using Waives.Http.Responses;
using Xunit;

namespace Waives.Http.Tests
{
    public class DocumentFacts : IDisposable
    {
        private readonly IHttpRequestSender _requestSender = Substitute.For<IHttpRequestSender>();
        private readonly Document _sut;
        private readonly string _readUrl;
        private readonly string _classifyUrl;
        private readonly string _selfUrl;
        private readonly string _classifierName;
        private readonly string _readResultsFilename;
        private readonly string _readResultsResponseContent;

        public DocumentFacts()
        {
            var documentId = "id";
            _classifierName = "classifier";

            _readUrl = $"/documents/{documentId}/reads";
            _classifyUrl = $"/documents/{documentId}/classify/{_classifierName}";
            _selfUrl = $"/documents/{documentId}";

            IDictionary<string, HalUri> behaviours = new Dictionary<string, HalUri>
            {
                { "document:read", new HalUri(new Uri(_readUrl, UriKind.Relative), false) },
                { "document:classify", new HalUri(new Uri(_classifyUrl, UriKind.Relative), true) },
                { "self", new HalUri(new Uri(_selfUrl, UriKind.Relative), false) },
            };

            _sut = new Document(_requestSender, behaviours, "id");

            _readResultsFilename = Path.GetTempFileName();
            _readResultsResponseContent = "some text that was read";
        }

        [Fact]
        public async Task Delete_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ASuccessResponse());

            await _sut.Delete();

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Delete &&
                    m.RequestUri.ToString() == _selfUrl));
        }

        [Fact]
        public async Task Delete_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AnErrorResponse());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Delete());
            Assert.Equal("Failed to delete the document.", exception.Message);
        }

        [Fact]
        public async Task Read_sends_read_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ASuccessResponse(), AGetReadResultsResponse());

            await _sut.Read(_readResultsFilename);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Put &&
                    m.RequestUri.ToString() == _readUrl));
        }

        [Fact]
        public async Task Read_sends_get_read_results_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ASuccessResponse(), AGetReadResultsResponse());

            await _sut.Read(_readResultsFilename);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Get &&
                    m.RequestUri.ToString() == _readUrl));
        }

        [Fact]
        public async Task Read_sends_get_read_results_request_with_specified_content_type()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ASuccessResponse(), AGetReadResultsResponse());

            var expectedContentType = "application/text";

            await _sut.Read(_readResultsFilename, expectedContentType);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Get &&
                    m.Headers.Accept.ToString() == expectedContentType));
        }

        [Fact]
        public async Task Read_gets_read_results_in_waives_format_if_content_type_is_not_specified()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ASuccessResponse(), AGetReadResultsResponse());

            await _sut.Read(_readResultsFilename);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Get &&
                    m.Headers.Accept.ToString() == ContentTypes.WaivesReadResults));
        }

        [Fact]
        public async Task Read_saves_contents_of_read_results_response_to_specified_file()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ASuccessResponse(), AGetReadResultsResponse());

            await _sut.Read(_readResultsFilename);

            var fileContents = await File.ReadAllTextAsync(_readResultsFilename);

            Assert.Equal(_readResultsResponseContent, fileContents);
        }

        [Fact]
        public async Task Read_throws_if_read_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AnErrorResponse(), AGetReadResultsResponse());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Read(_readResultsFilename));
            Assert.Equal("Failed initiating read on document.", exception.Message);
        }


        [Fact]
        public async Task Delete_throws_if_get_read_results_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(ASuccessResponse(), AnErrorResponse());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Read(_readResultsFilename));
            Assert.Equal("Failed retrieving document read results.", exception.Message);
        }

        [Fact]
        public async Task Classify_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AClassifyResponse());

            await _sut.Classify(_classifierName);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri.ToString() == _classifyUrl));
        }

        [Fact]
        public async Task Classify_returns_a_result_with_correct_properties_set()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AClassifyResponse());

            var result = await _sut.Classify(_classifierName);

            Assert.Equal("expectedDocumentType", result.DocumentType);
            Assert.Equal(2.85512137M, result.RelativeConfidence);
            Assert.True(result.IsConfident);
            Assert.Equal(5, result.DocumentTypeScores.Count());
            Assert.Equal("Assignment of Deed of Trust", result.DocumentTypeScores.First().DocumentType);
            Assert.Equal(61.4187M, result.DocumentTypeScores.First().Score);
        }

        [Fact]
        public async Task Classify_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(AnErrorResponse());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Classify(_classifierName));
            Assert.Equal($"Failed to classify the document with classifier '{_classifierName}'",
                exception.Message);
        }

        public void Dispose()
        {
            if (File.Exists(_readResultsFilename))
            {
                File.Delete(_readResultsFilename);
            }
        }

        private static HttpResponseMessage ASuccessResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private static HttpResponseMessage AnErrorResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private HttpResponseMessage AGetReadResultsResponse()
        {
            return
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_readResultsResponseContent)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("text/plain") }
                    },
                };
        }

        private static HttpResponseMessage AClassifyResponse()
        {
            return
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ClassifyResponse)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                    },
                };
        }

        private const string ClassifyResponse = @"{
	        ""document_id"": ""expectedDocumentId"",
            ""classification_results"": {
                ""document_type"": ""expectedDocumentType"",
                ""relative_confidence"": 2.85512137,
                ""is_confident"": true,
                ""document_type_scores"": [
                {
                    ""document_type"": ""Assignment of Deed of Trust"",
                    ""score"": 61.4187

                },
                {
                    ""document_type"": ""Notice of Default"",
                    ""score"": 32.94312
                },
                {
                    ""document_type"": ""Correspondence"",
                    ""score"": 28.2860489
                },
                {
                    ""document_type"": ""Deed of Trust"",
                    ""score"": 28.0011711
                },
                {
                    ""document_type"": ""Notice of Lien"",
                    ""score"": 27.9561481
                }
                ]
            }
        }";
    }
}