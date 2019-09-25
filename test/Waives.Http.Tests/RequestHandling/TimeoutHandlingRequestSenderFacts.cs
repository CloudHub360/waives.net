using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Waives.Http.RequestHandling;
using Xunit;

namespace Waives.Http.Tests.RequestHandling
{
    public class TimeoutHandlingRequestSenderFacts
    {
        private readonly IHttpRequestSender _sender;
        private readonly HttpRequestMessageTemplate _request;
        private readonly TimeoutHandlingRequestSender _sut;

        public TimeoutHandlingRequestSenderFacts()
        {
            _sender = Substitute.For<IHttpRequestSender>();
            _sut = new TimeoutHandlingRequestSender(_sender);

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

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task Throws_WaivesApiException_if_wrapped_sender_times_out(Exception senderException)
        {
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(senderException);

            await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Send(_request));
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task Includes_request_details_in_exception_if_wrapped_sender_times_out(Exception senderException)
        {
            _sender
                .Send(Arg.Any<HttpRequestMessageTemplate>())
                .Throws(senderException);

            var actualException = await Assert.ThrowsAsync<WaivesApiException>(() =>
                _sut.Send(_request));

            Assert.Contains(_request.Method.ToString(), actualException.Message);
            Assert.Contains(_request.RequestUri.ToString(), actualException.Message);
        }

        // If HttpClient times-out (client-side) then one of these exceptions is thrown
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> TimeoutExceptions()
        {
            yield return new object[] { new TaskCanceledException() };
            yield return new object[] { new OperationCanceledException() };
        }
    }
}