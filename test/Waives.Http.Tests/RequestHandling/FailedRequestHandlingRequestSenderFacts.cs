using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using Waives.Http.RequestHandling;
using Waives.Http.Tests.RequestHandling;
using Xunit;

namespace Waives.Http.Tests
{
    public class FailedRequestHandlingRequestSenderFacts
    {
        private readonly IHttpRequestSender _requestSender = Substitute.For<IHttpRequestSender>();
        private readonly FailedRequestHandlingRequestSender _sut;
        private readonly HttpRequestMessageTemplate _request =
            new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative));

        public FailedRequestHandlingRequestSenderFacts()
        {
            _sut = new FailedRequestHandlingRequestSender(_requestSender);
        }

        [Theory, MemberData(nameof(FailedStatusCodes))]
        public async Task Throw_WaivesApiException_on_failed_requests(HttpStatusCode statusCode)
        {
            _requestSender
                .Send(_request)
                .ReturnsForAnyArgs(Response.ErrorFrom(statusCode, _request));

            var exception = await Assert.ThrowsAsync<WaivesApiException>(() => _sut.Send(_request));
            Assert.Equal(Response.ErrorMessage, exception.Message);
        }

        [Theory, MemberData(nameof(SuccessStatusCodes))]
        public async Task Return_the_response_for_successful_requests(HttpStatusCode statusCode)
        {
            var expected = Response.SuccessFrom(statusCode, _request);
            _requestSender
                .Send(_request)
                .ReturnsForAnyArgs(expected);

            var actual = await _sut.Send(_request);

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> SuccessStatusCodes()
        {
            yield return new object[] { HttpStatusCode.OK };
            yield return new object[] { HttpStatusCode.Created };
            yield return new object[] { HttpStatusCode.Accepted };
            yield return new object[] { HttpStatusCode.NonAuthoritativeInformation };
            yield return new object[] { HttpStatusCode.NoContent };
            yield return new object[] { HttpStatusCode.ResetContent };
            yield return new object[] { HttpStatusCode.PartialContent };
        }

        public static IEnumerable<object[]> FailedStatusCodes()
        {
            // 1xx
            yield return new object[] { HttpStatusCode.Continue };
            yield return new object[] { HttpStatusCode.SwitchingProtocols };

            // 3xx
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

            // 4xx
            yield return new object[] { HttpStatusCode.BadRequest };
            yield return new object[] { HttpStatusCode.Unauthorized };
            yield return new object[] { HttpStatusCode.PaymentRequired };
            yield return new object[] { HttpStatusCode.Forbidden };
            yield return new object[] { HttpStatusCode.NotFound };
            yield return new object[] { HttpStatusCode.MethodNotAllowed };
            yield return new object[] { HttpStatusCode.NotAcceptable };
            yield return new object[] { HttpStatusCode.ProxyAuthenticationRequired };
            yield return new object[] { HttpStatusCode.RequestTimeout };
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

            // 5xx
            yield return new object[] { HttpStatusCode.InternalServerError };
            yield return new object[] { HttpStatusCode.NotImplemented };
            yield return new object[] { HttpStatusCode.BadGateway };
            yield return new object[] { HttpStatusCode.ServiceUnavailable };
            yield return new object[] { HttpStatusCode.GatewayTimeout };
            yield return new object[] { HttpStatusCode.HttpVersionNotSupported };
            yield return new object[] { HttpStatusCode.VariantAlsoNegotiates };
            yield return new object[] { HttpStatusCode.InsufficientStorage };
            yield return new object[] { HttpStatusCode.LoopDetected };
            yield return new object[] { HttpStatusCode.NotExtended };
            yield return new object[] { HttpStatusCode.NetworkAuthenticationRequired };
        }
    }
}