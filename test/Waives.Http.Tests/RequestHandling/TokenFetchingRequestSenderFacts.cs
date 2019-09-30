using System;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using Waives.Http.RequestHandling;
using Xunit;

namespace Waives.Http.Tests.RequestHandling
{
    public class TokenFetchingRequestSenderFacts
    {
        private readonly IHttpRequestSender _requestSender = Substitute.For<IHttpRequestSender>();
        private readonly TokenFetchingRequestSender _sut;

        public TokenFetchingRequestSenderFacts()
        {
            _sut = new TokenFetchingRequestSender(
                new AccessTokenService(
                    "clientId", "clientSecret",
                    _requestSender),
                _requestSender);

            _requestSender
                .SendAsync(Arg.Is<HttpRequestMessageTemplate>(
                    r => !r.Headers.ContainsKey("Authorization")))
                .Returns(ci => Response.GetToken(ci.Arg<HttpRequestMessageTemplate>()));
        }

        [Fact]
        public async Task Send_retrieves_an_access_token_for_the_request()
        {
            await _sut.SendAsync(new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative)));
            await _requestSender
                .Received(1)
                .SendAsync(Arg.Is<HttpRequestMessageTemplate>(
                    r => r.RequestUri == new Uri("/oauth/token", UriKind.Relative)));
        }

        [Fact]
        public async Task Send_retrieves_an_access_token_only_once_while_the_token_is_valid()
        {
            await _sut.SendAsync(new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative)));
            await _sut.SendAsync(new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative)));
            await _sut.SendAsync(new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative)));

            await _requestSender
                .Received(1)
                .SendAsync(Arg.Is<HttpRequestMessageTemplate>(
                    r => r.RequestUri == new Uri("/oauth/token", UriKind.Relative)));
        }

        [Fact]
        public async Task Send_adds_the_access_token_to_the_onward_request()
        {
            var template = new HttpRequestMessageTemplate(HttpMethod.Get, new Uri("/documents", UriKind.Relative));

            await _sut.SendAsync(template);

            await _requestSender
                .Received(1)
                .SendAsync(Arg.Is<HttpRequestMessageTemplate>(
                    r => r.RequestUri == template.RequestUri &&
                         r.Headers.ContainsKey("Authorization")));
        }
    }
}
