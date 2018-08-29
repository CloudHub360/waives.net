using System;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Waives.Http.Logging;
using Xunit;

namespace Waives.Http.Tests
{
    public class LoggingRequestSenderFacts
    {
        private readonly IHttpRequestSender _sender;
        private readonly ILogger _logger;
        private readonly LoggingRequestSender _sut;
        private readonly HttpRequestMessage _request;

        public LoggingRequestSenderFacts()
        {
            _sender = Substitute.For<IHttpRequestSender>();
            _logger = Substitute.For<ILogger>();
            _sut = new LoggingRequestSender(_sender, _logger);

            _request = new HttpRequestMessage(HttpMethod.Get, new Uri("/documents", UriKind.Relative));
        }

        [Fact]
        public async Task Returns_response_from_wrapped_sender()
        {
            var expectedResponse = Responses.Success();
            _sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(expectedResponse);

            var response = await _sut.Send(_request);

            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task Rethrows_original_exception_when_wrapped_sender_throws_exception()
        {
            var expectedException = new WaivesApiException();
            _sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Throws(expectedException);

            try
            {
                await _sut.Send(_request);
            }
            catch (WaivesApiException e)
            {
                Assert.Same(expectedException, e);
                return;
            }

            Assert.False(true);
        }

        [Fact]
        public async Task Logs_two_trace_messages_when_request_is_successful()
        {
            _sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.Success());

            await _sut.Send(_request);

            _logger.Received(2)
                .Log(LogLevel.Trace, Arg.Any<string>());
        }

        [Fact]
        public async Task Logs_a_trace_and_an_error_message_when_wrapped_sender_throws_exception()
        {
            var exception = new WaivesApiException("an error message");
            _sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Throws(exception);

            try
            {
                await _sut.Send(_request);
            }
            catch (WaivesApiException)
            {
                _logger.Received(1)
                    .Log(LogLevel.Trace, Arg.Any<string>());

                _logger.Received(1)
                    .Log(LogLevel.Error, Arg.Any<string>());

                return;
            }

            Assert.False(true);
        }

        [Fact]
        public async Task Logs_an_error_message_that_includes_the_exception_message()
        {
            var exception = new WaivesApiException("an error message");
            _sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Throws(exception);

            try
            {
                await _sut.Send(_request);
            }
            catch (WaivesApiException)
            {
                _logger.Received(1)
                    .Log(LogLevel.Error, Arg.Is<string>(m => m.Contains(exception.Message)));

                return;
            }

            Assert.False(true);
        }

        [Fact]
        public async Task Logs_an_error_message_that_includes_the_inner_exception_message_if_set()
        {
            var innerException = new Exception("inner message");
            var exception = new WaivesApiException("an error message", innerException);

            _sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Throws(exception);

            try
            {
                await _sut.Send(_request);
            }
            catch (WaivesApiException)
            {
                _logger.Received(1)
                    .Log(LogLevel.Error, Arg.Is<string>(m => m.Contains(innerException.Message)));

                return;
            }

            Assert.False(true);
        }
    }
}