using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog;
using Waives.Http.RequestHandling;
using Xunit;
using Xunit.Abstractions;

namespace Waives.Http.Tests.RequestHandling
{
    public class LoggingRequestSenderFacts : IDisposable
    {
        private readonly StringBuilder _consoleOutput = new StringBuilder();

        private ITestOutputHelper Console { get; }

        private readonly IHttpRequestSender _sender;
        private readonly LoggingRequestSender _sut;
        private readonly HttpRequestMessageTemplate _request;

        public LoggingRequestSenderFacts(ITestOutputHelper output)
        {
            Console = output;
            System.Console.SetOut(TextWriter.Synchronized(new StringWriter(_consoleOutput)));
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Verbose()
                .CreateLogger();

            _sender = Substitute.For<IHttpRequestSender>();
            _sut = new LoggingRequestSender(_sender);

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

            var result = _consoleOutput.ToString();
            Assert.Matches($"VRB.*Sending {_request.Method} request to {_request.RequestUri}", result);
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

            var result = _consoleOutput.ToString();
            Assert.Matches($"VRB.*Sending {_request.Method} request to {_request.RequestUri}", result);
        }

        [Fact]
        public async Task Logs_a_received_response_message_when_request_is_successful()
        {
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(ci => Response.Success(ci.Arg<HttpRequestMessageTemplate>()));

            await _sut.Send(_request);

            var result = _consoleOutput.ToString();
            Assert.Matches($"VRB.*Received response from {_request.Method} {_request.RequestUri} \\(200\\) \\(\\d+ ms\\)", result);
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

            var result = _consoleOutput.ToString();
            Assert.Matches($"ERR.*{exception.Message}", result);
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

            var result = _consoleOutput.ToString();
            Assert.Matches($"ERR.*{exception.Message}", result);
            Assert.Matches(@"Waives\.Http\.WaivesApiException: an error message", result);
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

            var result = _consoleOutput.ToString();
            Assert.Matches(
                $"ERR.*{exception.Message} Inner exception: {innerException.GetType()}: {innerException.Message}",
                result);
            Assert.Matches(@"Waives\.Http\.WaivesApiException: an error message ---> System\.Exception: inner message", result);
        }

        public void Dispose()
        {
            var logLines = _consoleOutput.ToString()
                .Split(Environment.NewLine)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select((e, i) => $"  {i + 1}. {e}");

            Console.WriteLine($"Received log messages:{Environment.NewLine}{string.Join(Environment.NewLine, logLines)}");
            Console.WriteLine("");
        }
    }
}