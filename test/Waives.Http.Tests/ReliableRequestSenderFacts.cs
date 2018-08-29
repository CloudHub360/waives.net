using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NSubstitute;
using Polly;
using Xunit;

namespace Waives.Http.Tests
{
    public class ReliableRequestSenderFacts
    {
        private readonly RetryLogger _retryLogger;
        private readonly HttpRequestMessage _request;

        public ReliableRequestSenderFacts()
        {
            _retryLogger = new RetryLogger();
            _request = new HttpRequestMessage(HttpMethod.Get, new Uri("/documents", UriKind.Relative));
        }

        [Theory]
        [InlineData(HttpStatusCode.RequestTimeout)]      //408
        [InlineData(HttpStatusCode.InternalServerError)] //500
        [InlineData(HttpStatusCode.NotImplemented)]      //501
        [InlineData(HttpStatusCode.BadGateway)]          //502
        [InlineData(HttpStatusCode.ServiceUnavailable)]  //503
        [InlineData(HttpStatusCode.GatewayTimeout)]      //504
        [InlineData(HttpStatusCode.HttpVersionNotSupported)] //505
        [InlineData(HttpStatusCode.VariantAlsoNegotiates)]   //506
        [InlineData(HttpStatusCode.InsufficientStorage)] //507
        [InlineData(HttpStatusCode.LoopDetected)]        //508
        [InlineData(HttpStatusCode.NotExtended)]         //509
        [InlineData(HttpStatusCode.NetworkAuthenticationRequired)] //510
        public async Task Send_retries_on_error_response(HttpStatusCode statusCode)
        {
            var sender = Substitute.For<IHttpRequestSender>();
            sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(
                    new HttpResponseMessage(statusCode),
                    Responses.Success());

            var sut = new ReliableRequestSender(
                _retryLogger.RetryAction,
                sender);

            await sut.Send(_request);

            Assert.True(_retryLogger.RetryActionCalled);
        }

        [Theory]
        [InlineData(HttpStatusCode.Continue)]         //100
        [InlineData(HttpStatusCode.OK)]               //200
        [InlineData(HttpStatusCode.MultipleChoices)]  //300
        [InlineData(HttpStatusCode.BadRequest)]       //400
        public async Task Send_does_not_retry_on_satisfactory_responses(HttpStatusCode statusCode)
        {
            var sender = Substitute.For<IHttpRequestSender>();
            sender
                .Send(Arg.Any<HttpRequestMessage>())
                .Returns(
                    new HttpResponseMessage(statusCode),
                    Responses.Success());

            var sut = new ReliableRequestSender(
                _retryLogger.RetryAction,
                sender);

            await sut.Send(_request);

            Assert.False(_retryLogger.RetryActionCalled);
        }

        [Fact]
        public async Task Send_retries_on_http_exception()
        {
            var sender = Substitute.For<IHttpRequestSender>();

            sender.Send(Arg.Any<HttpRequestMessage>())
                .Returns(x => throw new HttpRequestException(), x => Responses.Success());

            var sut = new ReliableRequestSender(
                _retryLogger.RetryAction,
                sender);

            await sut.Send(_request);

            Assert.True(_retryLogger.RetryActionCalled);
        }

        [Fact]
        public async Task Send_retries_on_waives_api_exception()
        {
            var sender = Substitute.For<IHttpRequestSender>();

            sender.Send(Arg.Any<HttpRequestMessage>())
                .Returns(x => throw new WaivesApiException(), x => Responses.Success());

            var sut = new ReliableRequestSender(
                _retryLogger.RetryAction,
                sender);

            await sut.Send(_request);

            Assert.True(_retryLogger.RetryActionCalled);
        }

        [Fact]
        public async Task Send_calls_the_wrapped_request_sender_with_a_properly_cloned_request()
        {
            var request = ARequestWithContentAndHeader();

            var sender = Substitute.For<IHttpRequestSender>();
            sender.Send(Arg.Any<HttpRequestMessage>())
                .Returns(Responses.Success());

            var sut = new ReliableRequestSender(
                _retryLogger.RetryAction,
                sender);

            await sut.Send(request);

            await sender
                .Received(1)
                .Send(Arg.Any<HttpRequestMessage>());

            await sender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessage>(m =>
                    RequestsAreEqual(request, m)));
        }

        private static HttpRequestMessage ARequestWithContentAndHeader()
        {
            return new HttpRequestMessage(HttpMethod.Get, new Uri("/documents", UriKind.Relative))
            {
                Content = new StringContent("some content")
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                },
                Properties =
                {
                    { "A property", "A property value"}
                }
            };
        }

        private bool RequestsAreEqual(HttpRequestMessage expectedRequest, HttpRequestMessage actualRequest)
        {
            Assert.Equal(expectedRequest.Method, actualRequest.Method);
            Assert.Equal(expectedRequest.RequestUri, actualRequest.RequestUri);

            Assert.Equal(expectedRequest.Headers, actualRequest.Headers);
            Assert.Equal(expectedRequest.Properties, actualRequest.Properties);

            var expectedContent = expectedRequest.Content.ReadAsByteArrayAsync().Result;
            var actualContent = actualRequest.Content.ReadAsByteArrayAsync().Result;
            Assert.True(expectedContent.SequenceEqual(actualContent));

            return true;
        }

        private class RetryLogger
        {
            internal bool RetryActionCalled { get; private set; }

            internal void RetryAction(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, int counter, Context context)
            {
                RetryActionCalled = true;
            }
        }
    }
}