using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog.Events;
using Waives.Http.RequestHandling;
using Waives.Http.Tests.Logging;
using Xunit;

namespace Waives.Http.Tests.RequestHandling
{
    public class LoggingRequestSenderFacts : IDisposable
    {
        private readonly IHttpRequestSender _sender;
        private readonly IList<LogEvent> _logEvents = new List<LogEvent>();
        private readonly IDisposable _logger;
        private readonly LoggingRequestSender _sut;
        private readonly HttpRequestMessageTemplate _request;

        public LoggingRequestSenderFacts()
        {
            _sender = Substitute.For<IHttpRequestSender>();
            _sut = new LoggingRequestSender(_sender);

            _logger = Logger.CaptureTo(_logEvents);

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
                .Returns(ci => Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Send(_request);

            _logEvents
                .HasMessage("Sending {RequestMethod} request to {RequestUri}")
                .AtLevel(LogEventLevel.Verbose)
                .Once()
                .WithPropertyValue("RequestMethod", $"\"{_request.Method}\"")
                .WithPropertyValue("RequestUri", _request.RequestUri);
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

            _logEvents
                .HasMessage("Sending {RequestMethod} request to {RequestUri}")
                .AtLevel(LogEventLevel.Verbose)
                .Once()
                .WithPropertyValue("RequestMethod", $"\"{_request.Method}\"")
                .WithPropertyValue("RequestUri", _request.RequestUri);
        }

        [Fact]
        public async Task Logs_a_received_response_message_when_request_is_successful()
        {
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Send(_request);

            _logEvents
                .HasMessage("Received response from {RequestMethod} {RequestUri} ({StatusCode}) ({ElapsedMilliseconds} ms)")
                .AtLevel(LogEventLevel.Verbose)
                .Once()
                .WithPropertyValue("RequestMethod", $"\"{_request.Method}\"")
                .WithPropertyValue("RequestUri", _request.RequestUri)
                .WithPropertyValue("StatusCode", 200)
                .WithProperty("ElapsedMilliseconds");
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

            _logEvents
                .HasMessage(exception.Message)
                .AtLevel(LogEventLevel.Error)
                .Once()
                .WithException(exception);
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

            _logEvents
                .HasMessage("{Message} Inner exception: {InnerExceptionMessage}")
                .AtLevel(LogEventLevel.Error)
                .Once()
                .WithException(exception)
                .WithPropertyValue("Message", $"\"{exception.Message}\"")
                .WithPropertyValue("InnerExceptionMessage", $"\"{innerException.Message}\"");
        }

        public void Dispose()
        {
            _logger.Dispose();
        }
    }
}