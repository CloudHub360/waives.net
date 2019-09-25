using System;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Waives.Http.Logging;
using Waives.Http.RequestHandling;
using Xunit;

namespace Waives.Http.Tests.RequestHandling
{
    public class LoggingRequestSenderFacts
    {
        private readonly IHttpRequestSender _sender;
        private readonly ILogger _logger;
        private readonly LoggingRequestSender _sut;
        private readonly HttpRequestMessageTemplate _request;

        public LoggingRequestSenderFacts()
        {
            _sender = Substitute.For<IHttpRequestSender>();
            _logger = Substitute.For<ILogger>();
            _sut = new LoggingRequestSender(_logger, _sender);

            _request = new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative));
        }

        [Fact]
        public async Task Returns_response_from_wrapped_sender()
        {
            var expectedResponse = Response.Success(_request);
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(expectedResponse);

            var response = await _sut.Send(_request);

            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task Rethrows_original_exception_when_wrapped_sender_throws_exception()
        {
            var expectedException = new WaivesApiException();
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(expectedException);

            var actualException = await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Send(_request));

            Assert.Same(expectedException, actualException);
        }

        [Fact]
        public async Task Logs_a_sending_request_message_when_request_is_successful()
        {
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => RequestHandling.Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Send(_request);

            _logger.Received(2)
                .Log(LogLevel.Trace, Arg.Any<string>());
        }

        [Fact]
        public async Task Logs_a_sending_request_message_when_request_is_not_successful()
        {
            var exception = new WaivesApiException("an error message");
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(exception);

            await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Send(_request));

            _logger.Received(1)
                .Log(LogLevel.Trace, Arg.Is<string>(m => m.Contains(_request.RequestUri.ToString())));
        }

        [Fact]
        public async Task Logs_a_received_response_message_when_request_is_successful()
        {
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => RequestHandling.Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Send(_request);

            _logger.Received(2)
                .Log(LogLevel.Trace, Arg.Any<string>());
        }

        [Fact]
        public async Task Logs_an_error_message_when_request_is_not_successful()
        {
            var exception = new WaivesApiException("an error message");
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(exception);

            await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Send(_request));

                _logger.Received(1)
                    .Log(LogLevel.Error, Arg.Any<string>());
        }

        [Fact]
        public async Task Logs_an_error_message_that_includes_the_exception_message()
        {
            var exception = new WaivesApiException("an error message");
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(exception);

            await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Send(_request));

            _logger.Received(1)
                    .Log(LogLevel.Error, Arg.Is<string>(m => m.Contains(exception.Message)));
        }

        [Fact]
        public async Task Logs_an_error_message_that_includes_the_inner_exception_message_if_set()
        {
            var innerException = new Exception("inner message");
            var exception = new WaivesApiException("an error message", innerException);

            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(exception);

            await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Send(_request));

            _logger.Received(1)
                    .Log(LogLevel.Error, Arg.Is<string>(m => m.Contains(innerException.Message)));
        }
    }
}