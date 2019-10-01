using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Waives.Http.RequestHandling
{
    internal class HttpRequestMessageTemplate
    {
        public HttpRequestMessageTemplate(HttpMethod method, Uri requestUri, IDictionary<string, string> headers) : this(method, requestUri)
        {
            Headers = headers;
        }

        public HttpRequestMessageTemplate(HttpMethod method, Uri requestUri)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            RequestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            Headers = new Dictionary<string, string>();
        }

        public HttpMethod Method { get; }
        public Uri RequestUri { get; }
        public HttpContent Content { get; set; }
        public IDictionary<string, string> Headers { get; }

        public HttpRequestMessage CreateRequest()
        {
            var request = new HttpRequestMessage(Method, RequestUri)
            {
                Content = Content
            };

            foreach (var header in Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return request;
        }
    }
}