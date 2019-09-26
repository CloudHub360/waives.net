using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Waives.Http.RequestHandling;
using Waives.Http.Responses;
using Waives.Http.Tests.RequestHandling;
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
        private readonly string _redactUrl;
        private readonly string _selfUrl;
        private readonly string _classifierName;
        private readonly string _extractorName;
        private readonly string _readResultsFilename;


        public DocumentFacts()
        {
            const string documentId = "id";
            _classifierName = "classifier";
            _extractorName = "extractor";

            _readUrl = $"/documents/{documentId}/reads";

            var templatedClassifyUrl = $"/documents/{documentId}/classify/" + "{classifier_name}";
            var templatedExtractUrl = $"/documents/{documentId}/extract/" + "{extractor_name}";

            _classifyUrl = $"/documents/{documentId}/classify/{_classifierName}";
            _extractUrl = $"/documents/{documentId}/extract/{_extractorName}";
            _redactUrl = $"/documents/{documentId}/redact";
            _selfUrl = $"/documents/{documentId}";

            IDictionary<string, HalUri> behaviours = new Dictionary<string, HalUri>
            {
                { "document:read", new HalUri(new Uri(_readUrl, UriKind.Relative), false) },
                { "document:classify", new HalUri(new Uri(templatedClassifyUrl, UriKind.Relative), true) },
                { "document:extract", new HalUri(new Uri(templatedExtractUrl, UriKind.Relative), true) },
                { "self", new HalUri(new Uri(_selfUrl, UriKind.Relative), false) }
            };

            _sut = new Document(_requestSender, "id", behaviours);

            _readResultsFilename = Path.GetTempFileName();
        }

        [Fact]
        public async Task Delete_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Delete();

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Delete &&
                    m.RequestUri.ToString() == _selfUrl));
        }

        [Fact]
        public async Task Delete_throws_if_response_is_not_success_code()
        {
            var exceptionMessage = $"Anonymous string {Guid.NewGuid()}";
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(exceptionMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Delete());
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task Read_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Read();

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Put &&
                    m.RequestUri.ToString() == _readUrl));
        }

        [Fact]
        public async Task Read_throws_if_response_is_not_success_code()
        {
            var exceptionMessage = $"Anonymous string {Guid.NewGuid()}";
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(exceptionMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Read());
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Theory]
        [InlineData(ReadResultsFormat.Text, "text/plain")]
        [InlineData(ReadResultsFormat.Pdf, "application/pdf")]
        [InlineData(ReadResultsFormat.WaivesDocument, "application/vnd.waives.resultformats.read+zip")]
        public async Task Get_read_results_sends_request_with_correct_url(ReadResultsFormat format, string expectedAcceptHeader)
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.GetReadResults(ci.Arg<HttpRequestMessageTemplate>(), $"Anonymous string {Guid.NewGuid()}"));

            await _sut.GetReadResults(_readResultsFilename, format);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Get &&
                    m.RequestUri.ToString() == _readUrl &&
                    m.Headers.Contains(new KeyValuePair<string, string>("Accept", expectedAcceptHeader))));
        }

        [Fact]
        public async Task Get_read_results_throws_if_response_is_not_success_code()
        {
            var exceptionMessage = $"Anonymous string {Guid.NewGuid()}";
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(exceptionMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.GetReadResults(_readResultsFilename, ReadResultsFormat.Pdf));
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task Get_read_results_writes_response_body_to_stream()
        {
            var readResults = $"Read results {Guid.NewGuid()}";

            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.GetReadResults(ci.Arg<HttpRequestMessageTemplate>(), readResults));

            using (var resultsStream = new MemoryStream())
            using (var sr = new StreamReader(resultsStream))
            {
                await _sut.GetReadResults(resultsStream, ReadResultsFormat.Text);

                resultsStream.Position = 0;
                Assert.Equal(readResults, sr.ReadToEnd());
            }
        }

        [Fact]
        public async Task Get_read_results_writes_response_body_to_file()
        {
            var readResults = $"Read results {Guid.NewGuid()}";

            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.GetReadResults(ci.Arg<HttpRequestMessageTemplate>(), readResults));

            await _sut.GetReadResults(_readResultsFilename, ReadResultsFormat.Text);


            Assert.Equal(readResults, await File.ReadAllTextAsync(_readResultsFilename));
        }

        [Fact]
        public async Task Classify_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Classify(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Classify(_classifierName);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri.ToString() == _classifyUrl));
        }

        [Fact]
        public async Task Classify_returns_a_result_with_correct_properties_set()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Classify(ci.Arg<HttpRequestMessageTemplate>()));

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
            var exceptionMessage = $"Anonymous string {Guid.NewGuid()}";
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(exceptionMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Classify(_classifierName));
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task Extract_sends_request_with_correct_uri()
        {
            var exceptionMessage = $"Anonymous string {Guid.NewGuid()}";
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Extract(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Extract(_extractorName);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri.ToString() == _extractUrl));
        }

        [Fact]
        public async Task Extract_returns_a_result_with_correct_properties_set()
        {
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Extract(ci.Arg<HttpRequestMessageTemplate>()));

            var response = await _sut.Extract(_extractorName);


            var extractionPage = response.DocumentMetadata.Pages.First();

            Assert.Equal(1, response.DocumentMetadata.PageCount);
            Assert.Equal(1, extractionPage.PageNumber);
            Assert.Equal(611.0, extractionPage.Width);
            Assert.Equal(1008.0, extractionPage.Height);

            var fieldResult = response.FieldResults.First();
            Assert.Equal("Amount", fieldResult.FieldName);
            Assert.False(fieldResult.Rejected);
            Assert.Equal("None", fieldResult.RejectReason);

            var primaryResult = fieldResult.Result;
            Assert.Equal("$5.50", primaryResult.Text);
            Assert.Equal(100.0, primaryResult.ProximityScore);
            Assert.Equal(100.0, primaryResult.MatchScore);
            Assert.Equal(100.0, primaryResult.TextScore);

            var primaryResultArea = primaryResult.Areas.First();
            Assert.Equal(558.7115, primaryResultArea.Top);
            Assert.Equal(276.48, primaryResultArea.Left);
            Assert.Equal(571.1989, primaryResultArea.Bottom);
            Assert.Equal(298.58, primaryResultArea.Right);
            Assert.Equal(1, primaryResultArea.PageNumber);

            var firstAlternativeResult = fieldResult.Alternatives.First();
            Assert.Equal("$10.50", firstAlternativeResult.Text);

            var firstAlternativeResultArea = firstAlternativeResult.Areas.First();
            Assert.Equal(123.4567, firstAlternativeResultArea.Top);
        }

        [Fact]
        public async Task Extract_throws_if_response_is_not_success_code()
        {
            var exceptionMessage = $"Anonymous string {Guid.NewGuid()}";
            _requestSender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(new WaivesApiException(exceptionMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Extract(_extractorName));
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task Redact_sends_request_with_correct_url()
        {
            _requestSender
                .Send(Arg.Is<HttpRequestMessageTemplate>(t => t.RequestUri.ToString().Contains("extract")))
                .Returns(ci => Response.Extract(ci.Arg<HttpRequestMessageTemplate>()));

            _requestSender
                .Send(Arg.Is<HttpRequestMessageTemplate>(t => t.RequestUri.ToString().Contains("redact")))
                .Returns(ci => Response.Redact(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Redact(_extractorName);

            await _requestSender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri.ToString() == _redactUrl));
        }

        [Fact]
        public async Task Redact_returns_a_stream()
        {
            _requestSender
                .Send(Arg.Is<HttpRequestMessageTemplate>(t => t.RequestUri.ToString().Contains("extract")))
                .Returns(ci => Response.Extract(ci.Arg<HttpRequestMessageTemplate>()));

            _requestSender
                .Send(Arg.Is<HttpRequestMessageTemplate>(t => t.RequestUri.ToString().Contains("redact")))
                .Returns(ci => Response.Redact(ci.Arg<HttpRequestMessageTemplate>()));

            var response = await _sut.Redact(_extractorName);

            var result = new byte[3].AsMemory();
            response.Read(result.Span);

            Assert.Equal(new byte[] { 1, 2, 3 }, result.ToArray());
        }

        [Fact]
        public async Task Redact_throws_if_response_is_not_success_code()
        {
            _requestSender
                .Send(Arg.Is<HttpRequestMessageTemplate>(t => t.RequestUri.ToString().Contains("extract")))
                .Returns(ci => Response.Extract(ci.Arg<HttpRequestMessageTemplate>()));

            var exceptionMessage = $"Anonymous string {Guid.NewGuid()}";
            _requestSender
                .Send(Arg.Is<HttpRequestMessageTemplate>(t => t.RequestUri.ToString().Contains("redact")))
                .Throws(new WaivesApiException(exceptionMessage));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Redact(_extractorName));
            Assert.Equal(exceptionMessage, exception.Message);
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