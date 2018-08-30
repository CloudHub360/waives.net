﻿using System;
using System.Collections.Generic;
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
        private readonly HttpRequestMessageTemplate _request;

        public ReliableRequestSenderFacts()
        {
            _retryLogger = new RetryLogger();
            _request = new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative));
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
                .Send(Arg.Any<HttpRequestMessageTemplate>())
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
        [MemberData(nameof(NoRetryHttpStatusCodes))]
        public async Task Send_does_not_retry_on_satisfactory_responses(HttpStatusCode statusCode)
        {
            var sender = Substitute.For<IHttpRequestSender>();
            sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
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
        public async Task Send_retries_on_waives_api_exception()
        {
            var sender = Substitute.For<IHttpRequestSender>();

            sender.Send(Arg.Any<HttpRequestMessageTemplate>())
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
            sender.Send(Arg.Any<HttpRequestMessageTemplate>())
                .Returns(Responses.Success());

            var sut = new ReliableRequestSender(
                _retryLogger.RetryAction,
                sender);

            await sut.Send(request);

            await sender
                .Received(1)
                .Send(Arg.Any<HttpRequestMessageTemplate>());

            await sender
                .Received(1)
                .Send(Arg.Is<HttpRequestMessageTemplate>(m =>
                    RequestsAreEqual(request, m)));
        }

        private static HttpRequestMessageTemplate ARequestWithContentAndHeader()
        {
            return new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative))
            {
                Content = new StringContent("some content")
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                }
            };
        }

        private bool RequestsAreEqual(HttpRequestMessageTemplate expectedRequest, HttpRequestMessageTemplate actualRequest)
        {
            Assert.Equal(expectedRequest.Method, actualRequest.Method);
            Assert.Equal(expectedRequest.RequestUri, actualRequest.RequestUri);

            Assert.Equal(expectedRequest.Headers, actualRequest.Headers);

            var expectedContent = expectedRequest.Content.ReadAsByteArrayAsync().Result;
            var actualContent = actualRequest.Content.ReadAsByteArrayAsync().Result;
            Assert.True(expectedContent.SequenceEqual(actualContent));

            return true;
        }

        public static IEnumerable<object[]> NoRetryHttpStatusCodes()
        {
            yield return new object[] { HttpStatusCode.Continue };
            yield return new object[] { HttpStatusCode.SwitchingProtocols };
            yield return new object[] { HttpStatusCode.OK };
            yield return new object[] { HttpStatusCode.Created };
            yield return new object[] { HttpStatusCode.Accepted };
            yield return new object[] { HttpStatusCode.NonAuthoritativeInformation };
            yield return new object[] { HttpStatusCode.NoContent };
            yield return new object[] { HttpStatusCode.ResetContent };
            yield return new object[] { HttpStatusCode.PartialContent };
            yield return new object[] { HttpStatusCode.MultipleChoices };
            yield return new object[] { HttpStatusCode.Ambiguous };
            yield return new object[] { HttpStatusCode.MovedPermanently };
            yield return new object[] { HttpStatusCode.Moved };
            yield return new object[] { HttpStatusCode.Found };
            yield return new object[] { HttpStatusCode.Redirect };
            yield return new object[] { HttpStatusCode.SeeOther };
            yield return new object[] { HttpStatusCode.NotModified };
            yield return new object[] { HttpStatusCode.UseProxy };
            yield return new object[] { HttpStatusCode.Unused };
            yield return new object[] { HttpStatusCode.TemporaryRedirect };
            yield return new object[] { HttpStatusCode.RedirectKeepVerb };
            yield return new object[] { HttpStatusCode.BadRequest };
            yield return new object[] { HttpStatusCode.Unauthorized };
            yield return new object[] { HttpStatusCode.PaymentRequired };
            yield return new object[] { HttpStatusCode.Forbidden };
            yield return new object[] { HttpStatusCode.NotFound };
            yield return new object[] { HttpStatusCode.MethodNotAllowed };
            yield return new object[] { HttpStatusCode.NotAcceptable };
            yield return new object[] { HttpStatusCode.ProxyAuthenticationRequired };
            yield return new object[] { HttpStatusCode.Conflict };
            yield return new object[] { HttpStatusCode.Gone };
            yield return new object[] { HttpStatusCode.LengthRequired };
            yield return new object[] { HttpStatusCode.PreconditionFailed };
            yield return new object[] { HttpStatusCode.RequestEntityTooLarge };
            yield return new object[] { HttpStatusCode.RequestUriTooLong };
            yield return new object[] { HttpStatusCode.UnsupportedMediaType };
            yield return new object[] { HttpStatusCode.RequestedRangeNotSatisfiable };
            yield return new object[] { HttpStatusCode.ExpectationFailed };
            yield return new object[] { HttpStatusCode.UpgradeRequired };
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