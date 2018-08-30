using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private readonly string _extractUrl;
        private readonly string _selfUrl;
        private readonly string _classifierName;
        private readonly string _extractorName;
        private readonly string _readResultsFilename;
        private readonly string _readResultsContent;

        public DocumentFacts()
        {
            const string documentId = "id";
            _classifierName = "classifier";
            _extractorName = "extractor";

            _readUrl = $"/documents/{documentId}/reads";
            _classifyUrl = $"/documents/{documentId}/classify/{_classifierName}";
            _extractUrl = $"/documents/{documentId}/extract/{_extractorName}";
            _selfUrl = $"/documents/{documentId}";

            IDictionary<string, HalUri> behaviours = new Dictionary<string, HalUri>
            {
                { "document:read", new HalUri(new Uri(_readUrl, UriKind.Relative), false) },
                { "document:classify", new HalUri(new Uri(_classifyUrl, UriKind.Relative), true) },
                { "document:extract", new HalUri(new Uri(_extractUrl, UriKind.Relative), true) },
                { "self", new HalUri(new Uri(_selfUrl, UriKind.Relative), false) },
            };

            _sut = new Document(_requestSender, "id", behaviours);

            _readResultsFilename = Path.GetTempFileName();
            _readResultsContent = "some text that was read";
        }

        [Fact]
        public async Task Delete_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.Success());

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
                .Returns(Responses.ErrorWithMessage());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Delete());
            Assert.Equal("Failed to delete the document.", exception.Message);
        }

        [Fact]
        public async Task Read_sends_read_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.Success(), Responses.GetReadResults(_readResultsContent));

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
                .Returns(Responses.Success(), Responses.GetReadResults(_readResultsContent));

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
                .Returns(Responses.Success(), Responses.GetReadResults(_readResultsContent));

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
                .Returns(Responses.Success(), Responses.GetReadResults(_readResultsContent));

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
                .Returns(Responses.Success(), Responses.GetReadResults(_readResultsContent));

            await _sut.Read(_readResultsFilename);

            var fileContents = await File.ReadAllTextAsync(_readResultsFilename);

            Assert.Equal(_readResultsContent, fileContents);
        }

        [Fact]
        public async Task Read_throws_if_read_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.ErrorWithMessage(), Responses.GetReadResults(_readResultsContent));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Read(_readResultsFilename));
            Assert.Equal("Failed initiating read on document.", exception.Message);
        }


        [Fact]
        public async Task Delete_throws_if_get_read_results_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.Success(), Responses.ErrorWithMessage());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Read(_readResultsFilename));
            Assert.Equal("Failed retrieving document read results.", exception.Message);
        }

        [Fact]
        public async Task Classify_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.Classify());

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
                .Returns(Responses.Classify());

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
                .Returns(Responses.ErrorWithMessage());

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Classify(_classifierName));
            Assert.Equal($"Failed to classify the document with classifier '{_classifierName}'",
                exception.Message);
        }

        [Fact]
        public async Task Extract_sends_request_with_correct_uri()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.Extract());

            await _sut.Extract(_extractorName);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri.ToString() == _extractUrl));
        }

        public void Dispose()
        {
            if (File.Exists(_readResultsFilename))
            {
                File.Delete(_readResultsFilename);
            }
        }
    }
}