using System;
using System.Collections.Generic;
using System.Net.Http;
using Waives.Http.RequestHandling;
using Xunit;

namespace Waives.Http.Tests.RequestHandling
{
    public class HttpRequestMessageTemplateFacts
    {
        [Theory]
        [MemberData(nameof(TemplateInputs))]
        public void Sets_method_and_uri_on_request(HttpMethod method, Uri uri)
        {
            var template = new HttpRequestMessageTemplate(
                method,
                uri);

            var request = template.CreateRequest();

            Assert.Equal(method, request.Method);
            Assert.Equal(uri, request.RequestUri);
        }

        [Fact]
        public void Sets_content_on_request()
        {
            const string expectedContents = "some contents";
            var template = new HttpRequestMessageTemplate(HttpMethod.Post, new Uri("/some-url", UriKind.Relative))
            {
                Content = new StringContent(expectedContents)
            };

            var request = template.CreateRequest();

            var actualContents = request.Content.ReadAsStringAsync().Result;

            Assert.Equal(expectedContents, actualContents);
        }

        [Fact]
        public void Sets_headers_on_request()
        {
            const string headerKey = "Accept";
            const string headerValue = "text/plain";

            var template = new HttpRequestMessageTemplate(HttpMethod.Post, new Uri("/some-url", UriKind.Relative));
            template.Headers.Add(new KeyValuePair<string, string>(headerKey, headerValue));

            var request = template.CreateRequest();

            Assert.Single(request.Headers);
        }

        public static IEnumerable<object[]> TemplateInputs()
        {
            yield return new object[] {HttpMethod.Post, new Uri("/some-url", UriKind.Relative)};
            yield return new object[] {HttpMethod.Get, new Uri("/another-url", UriKind.Relative)};
        }
    }
}